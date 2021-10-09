using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExamSaver.Services
{
    public class ExamService
    {
        private readonly DatabaseContext databaseContext;
        private readonly UserService userService;

        public ExamService(DatabaseContext databaseContext, UserService userService)
        {
            this.databaseContext = databaseContext;
            this.userService = userService;
        }

        public void SetExam(string token, ExamDTO examDTO)
        {
            int userId = userService.GetUserIdFromToken(token);

            CheckExamSetValidity(examDTO, userId);

            databaseContext.Add(new Exam()
            {
                StartTime = examDTO.StartTime,
                EndTime = examDTO.EndTime,
                SubjectId = examDTO.SubjectId
            });

            databaseContext.SaveChanges();
        }

        private void CheckExamSetValidity(ExamDTO examDTO, int userId)
        {
            if (examDTO.StartTime >= examDTO.EndTime)
            {
                throw new BadRequestException("Starting time must be greater than ending time");
            }

            CheckUserTeachesSubject(userId, examDTO.SubjectId);

            Exam exam = databaseContext
                .Exams
                .Where(exam => exam.SubjectId == examDTO.SubjectId
                            && exam.EndTime > DateTime.Now)
                .FirstOrDefault();

            if (exam != null)
            {
                throw new BadRequestException("Exam is already created and not finished yet");
            }
        }

        public IList<ExamDTO> GetHoldingExams(string token)
        {
            return GetExams(userService.GetUserIdFromToken(token), SubjectRelationType.TEACHING);
        }

        public IList<ExamDTO> GetTakingExams(string token)
        {
            return GetExams(userService.GetUserIdFromToken(token), SubjectRelationType.ATTENDING);
        }

        public IList<ExamDTO> GetExams(int studentId, SubjectRelationType subjectRelationType)
        {
            DateTime now = DateTime.Now;

            IList<Exam> exams = databaseContext
                .Exams
                .Include(exam => exam.Subject)
                .Join(
                    databaseContext.UsersSubjects,
                    exam => exam.SubjectId,
                    userSubject => userSubject.SubjectId,
                    (exam, userSubject) => new { exam, userSubject }
                )
                .Where(res => res.userSubject.UserId == studentId
                           && res.userSubject.SubjectRelation == subjectRelationType
                           && (subjectRelationType == SubjectRelationType.TEACHING || res.exam.StartTime <= now && res.exam.EndTime >= now))
                .Select(res => res.exam)
                .ToList();

            return exams.Select((exam) =>
            {
                return new ExamDTO()
                {
                    Id = exam.Id,
                    StartTime = exam.StartTime,
                    EndTime = exam.EndTime,
                    SubjectId = exam.SubjectId,
                    SubjectName = exam.Subject.Name
                };
            }).ToList();
        }

        public IList<StudentExamDTO> GetStudentExams(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = databaseContext.Exams.Find(examId);

            if (exam == null)
            {
                throw new BadRequestException($"Exam with id '{examId}' doesn't exist");
            }

            CheckUserTeachesSubject(userId, exam.SubjectId);

            return databaseContext
                .StudentsExams
                .Include(studentExam => studentExam.Student)
                .ThenInclude(student => student.User)
                .Where(studentExam => studentExam.ExamId == examId)
                .Select(studentExam => new StudentExamDTO()
                {
                    StudentId = studentExam.StudentId,
                    ExamId = studentExam.ExamId,
                    FirstName = studentExam.Student.User.FirstName,
                    LastName = studentExam.Student.User.LastName,
                    Index = studentExam.Student.Index,
                    UploadTime = studentExam.UploadTime,
                    ExamPath = studentExam.ExamPath
                })
                .ToList();
        }

        private void CheckUserTeachesSubject(int userId, int subjectId)
        {
            UserSubject userSubject = databaseContext
               .UsersSubjects
               .Include(userSubject => userSubject.Subject)
               .Where(userSubject => userSubject.UserId == userId
                                  && userSubject.SubjectId == subjectId
                                  && userSubject.SubjectRelation == SubjectRelationType.TEACHING)
               .FirstOrDefault();

            if (userSubject == null)
            {
                throw new BadRequestException("User doesn't teach provided subject");
            }
        }

        public void SaveExam(IFormCollection form, int examId, string token)
        {
            int userId = userService.GetUserIdFromToken(token);
            Student student = databaseContext
                .Students
                .Include(student => student.User)
                .Where(student => student.Id == userId)
                .FirstOrDefault();

            if (student == null)
            {
                throw new UserNotFoundException($"Student with id '{userId}' not found");
            }

            Exam exam = databaseContext
                .Exams
                .Include(exam => exam.Subject)
                .FirstOrDefault(exam => exam.Id == examId);

            CheckExamSubmitValidity(form, exam, student);

            foreach (IFormFile file in form.Files)
            {
                string resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), Constant.EXAMS_DIRECTORY_PATH);
                string studentResourceIdentifier = Util.GetStudentResourceIdentifier(student, examId);
                string studentExamPath = Path.Combine(resourcesPath, studentResourceIdentifier);

                if (!Directory.Exists(studentExamPath))
                {
                    Directory.CreateDirectory(studentExamPath);
                }
                else
                {
                    DeleteExistingContent(studentExamPath);
                }

                string filePath = Path.Combine(studentExamPath, $"{studentResourceIdentifier}.zip");

                using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string fileExtension = Path.GetExtension(fileName);

                if (fileExtension == ".zip")
                {
                    ExtractZipFile(filePath);
                }

                databaseContext.Add(new StudentExam()
                {
                    Exam = exam,
                    ExamPath = studentExamPath,
                    Student = student,
                    UploadTime = DateTime.Now
                });

                //TODO UNCOMMENT
                //databaseContext.SaveChanges();
            }
        }

        //TODO Check if files should be extracted or kept in .zip format
        private void ExtractZipFile(string filePath)
        {
            try
            {
                ZipArchive zipArchive = ZipFile.Open(filePath, ZipArchiveMode.Read);

                foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
                {

                }
            }
            catch (Exception ex)
            {
                if (ex is NotSupportedException || ex is InvalidDataException)
                {
                    throw new BadRequestException(ex.Message, ex);
                }

                throw;
            }
        }

        private void CheckExamSubmitValidity(IFormCollection form, Exam exam, Student student)
        {
            if (form.Files.Count == 0 || form.Files.Count > 1)
            {
                throw new BadRequestException("One file must be provided");
            }

            if (exam == null)
            {
                throw new BadRequestException("Exam not found");
            }

            UserSubject userSubject = databaseContext
                .UsersSubjects
                .Where(userSubject => userSubject.UserId == student.Id
                                        && userSubject.SubjectId == exam.SubjectId
                                        && userSubject.SubjectRelation == SubjectRelationType.ATTENDING)
                .FirstOrDefault();

            if (userSubject == null)
            {
                throw new BadRequestException($"Student with index '{student.Index}' doesn't attend to subject '{exam.Subject.Name}'");
            }

            if (exam.StartTime > DateTime.Now)
            {
                throw new BadRequestException("Exam hasn't started yet");
            }

            if (exam.EndTime < DateTime.Now)
            {
                throw new BadRequestException("Exam has ended");
            }
        }

        private void DeleteExistingContent(string directoryPath)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(directoryPath);

            foreach (FileInfo file in directoryInfo.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo subDirectory in directoryInfo.EnumerateDirectories())
            {
                subDirectory.Delete(true);
            }
        }
    }
}
