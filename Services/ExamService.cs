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
        private readonly FileService fileService;

        public ExamService(DatabaseContext databaseContext, UserService userService, FileService fileService)
        {
            this.databaseContext = databaseContext;
            this.userService = userService;
            this.fileService = fileService;
        }

        public void CreateExam(string token, ExamDTO examDTO)
        {
            int userId = userService.GetUserIdFromToken(token);

            CheckExamCreationValidity(examDTO, userId);

            databaseContext.Add(new Exam()
            {
                StartTime = examDTO.StartTime,
                EndTime = examDTO.EndTime,
                SubjectId = examDTO.SubjectId
            });

            databaseContext.SaveChanges();
        }

        public void UpdateExam(string token, ExamDTO examDTO, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            CheckExamUpdateValidity(examDTO, userId);

            Exam exam = new Exam()
            {
                Id = examId,
                StartTime = examDTO.StartTime,
                EndTime = examDTO.EndTime,
                SubjectId = examDTO.SubjectId
            };

            databaseContext.Attach(exam);
            databaseContext.Update(exam);

            databaseContext.SaveChanges();
        }

        private void CheckExamCreationValidity(ExamDTO examDTO, int userId)
        {
            CheckExamUpdateValidity(examDTO, userId);

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

        private void CheckExamUpdateValidity(ExamDTO examDTO, int userId)
        {
            CheckUserTeachesSubject(userId, examDTO.SubjectId);
            
            if (examDTO.StartTime >= examDTO.EndTime)
            {
                throw new BadRequestException("Starting time must be greater than ending time");
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

        public IList<StudentExamDTO> GetExamStudents(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = databaseContext.Exams.Find(examId);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' doesn't exist");
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

        public IList<FileInfoDTO> GetStudentExamFileTree(string token, int examId, int studentId, string fileTreePath)
        {
            int userId = userService.GetUserIdFromToken(token);

            Subject subject = GetExamSubject(examId);

            CheckUserTeachesSubject(userId, subject.Id);

            StudentExam studentExam = GetStudentExam(examId, studentId);

            return fileService.GetFileTree(fileTreePath, studentExam);
        }

        public FileDTO GetStudentExamFile(string token, int examId, int studentId, string fileTreePath)
        {
            int userId = userService.GetUserIdFromToken(token);

            Subject subject = GetExamSubject(examId);

            CheckUserTeachesSubject(userId, subject.Id);

            StudentExam studentExam = GetStudentExam(examId, studentId);

            return fileService.GetFile(fileTreePath, studentExam);
        }

        private Subject GetExamSubject(int examId)
        {
            Subject subject = databaseContext
                .Exams
                .Include(exam => exam.Subject)
                .Where(exam => exam.Id == examId)
                .Select(exam => exam.Subject)
                .FirstOrDefault();

            if (subject == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' not found");
            }

            return subject;
        }

        private void CheckUserTeachesSubject(int userId, int subjectId)
        {
            UserSubject userSubject = databaseContext
               .UsersSubjects
               .Where(userSubject => userSubject.UserId == userId
                                  && userSubject.SubjectId == subjectId
                                  && userSubject.SubjectRelation == SubjectRelationType.TEACHING)
               .FirstOrDefault();

            if (userSubject == null)
            {
                throw new BadRequestException($"User with id '{userId}' doesn't teach subject with id '{subjectId}'");
            }
        }

        private StudentExam GetStudentExam(int examId, int studentId)
        {
            StudentExam studentExam = databaseContext
                .StudentsExams
                .Include(studentExam => studentExam.Student)
                .ThenInclude(student => student.User)
                .Where(studentExam => studentExam.ExamId == examId
                                   && studentExam.StudentId == studentId)
                .FirstOrDefault();

            if (studentExam == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' for student with id '{studentId}' not found");
            }

            return studentExam;
        }

        public void SubmitExam(string token, IFormCollection form, int examId)
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
                .FirstOrDefault(exam => exam.Id == examId);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' not found");
            }

            CheckExamSubmitValidity(form, exam, student);

            fileService.SaveFile(form.Files[0], exam, student, out string studentExamFilePath);

            StudentExam studentExam = new StudentExam()
            {
                Exam = exam,
                ExamPath = studentExamFilePath,
                Student = student,
                UploadTime = DateTime.Now
            };

            AddUpdateStudentExam(studentExam);

            databaseContext.SaveChanges();
        }

        private void AddUpdateStudentExam(StudentExam studentExamModified)
        {
            StudentExam studentExamFound = databaseContext
                .StudentsExams
                .Where(studentExam => studentExam.ExamId == studentExamModified.Exam.Id
                                   && studentExam.StudentId == studentExamModified.Student.Id)
                .FirstOrDefault();

            if (studentExamFound == null)
            {
                databaseContext.Add(studentExamModified);
            }
            else
            {
                studentExamFound.ExamPath = studentExamModified.ExamPath;
                studentExamFound.UploadTime = DateTime.Now;
            }
        }

        private void CheckExamSubmitValidity(IFormCollection form, Exam exam, Student student)
        {
            if (form.Files.Count == 0 || form.Files.Count > 1)
            {
                throw new BadRequestException("One file must be provided");
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
    }
}
