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
using System.Linq;
using System.Threading;

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

        public void AddExam(string token, ExamDTO examDTO)
        {
            int userId = userService.GetUserIdFromToken(token);

            CheckExamAddValidity(userId, examDTO);

            databaseContext.Add(new Exam()
            {
                StartTime = examDTO.StartTime,
                EndTime = examDTO.EndTime,
                SubjectId = examDTO.SubjectId
            });

            databaseContext.SaveChanges();
        }

        public void UpdateExam(string token, int examId, ExamDTO examDTO)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = GetExam(examId);

            CheckExamUpdateValidity(userId, examDTO);

            exam.StartTime = examDTO.StartTime;
            exam.EndTime = examDTO.EndTime;

            databaseContext.Update(exam);

            databaseContext.SaveChanges();
        }

        public void DeleteExam(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = GetExam(examId);

            CheckUserTeachesSubject(userId, exam.SubjectId);

            List<StudentExam> studentExams = GetStudentExamsQuery(examId).ToList();

            foreach (StudentExam studentExam in studentExams)
            {
                fileService.DeleteStudentExamFile(studentExam);
            }

            databaseContext.Exams.Remove(exam);

            databaseContext.SaveChanges();
        }

        private void CheckExamAddValidity(int userId, ExamDTO examDTO)
        {
            CheckExamUpdateValidity(userId, examDTO);

            Exam exam = databaseContext
                .Exams
                .Where(exam => exam.SubjectId == examDTO.SubjectId
                            && (examDTO.StartTime >= exam.StartTime && examDTO.StartTime <= exam.EndTime
                               || examDTO.EndTime >= exam.StartTime && examDTO.EndTime <= exam.EndTime))
                .FirstOrDefault();

            if (exam != null)
            {
                throw new BadRequestException("Exam is overlapping with an existing exam");
            }
        }

        private void CheckExamUpdateValidity(int userId, ExamDTO examDTO)
        {
            CheckUserTeachesSubject(userId, examDTO.SubjectId);

            if (examDTO.StartTime >= examDTO.EndTime)
            {
                throw new BadRequestException("Starting time must be greater than ending time");
            }
        }

        public void SubmitWork(string token, int examId, IFormCollection form)
        {
            int userId = userService.GetUserIdFromToken(token);

            Student student = databaseContext
                .Students
                .Include(student => student.User)
                .Where(student => student.Id == userId)
                .FirstOrDefault();

            if (student == null)
            {
                throw new UserNotFoundException("Authenticated user is not a student");
            }

            Exam exam = GetExam(examId);

            CheckExamSubmitValidity(form, exam, student);

            string studentExamFilePath = fileService.SaveFile(form.Files[0], exam, student);

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
            CheckUserAttendsSubject(student.Id, exam.SubjectId);

            if (form.Files.Count == 0 || form.Files.Count > 1)
            {
                throw new BadRequestException("One file must be provided");
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

        public PagedList<ExamDTO> GetHoldingExams(string token, int page)
        {
            int userId = userService.GetUserIdFromToken(token);

            return GetExams(userId, SubjectRelationType.TEACHING, page);
        }

        public ExamDTO GetHoldingExam(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = GetExam(examId);

            CheckUserTeachesSubject(userId, exam.SubjectId);

            return ExamDTO.FromEntity(exam);
        }

        public PagedList<ExamDTO> GetTakingExams(string token, int page)
        {
            int userId = userService.GetUserIdFromToken(token);

            return GetExams(userId, SubjectRelationType.ATTENDING, page);
        }

        public ExamDTO GetTakingExam(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = GetExam(examId);

            CheckUserAttendsSubject(userId, exam.SubjectId);

            return ExamDTO.FromEntity(exam);
        }

        public Exam GetExam(int examId)
        {
            Exam exam = databaseContext.Exams
                .Include(exam => exam.Subject)
                .Where(exam => exam.Id == examId)
                .FirstOrDefault();

            if (exam == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' is not found");
            }

            return exam;
        }

        private PagedList<ExamDTO> GetExams(int userId, SubjectRelationType subjectRelationType, int page = 1)
        {
            DateTime now = DateTime.Now;

            return databaseContext
                .Exams
                .Include(exam => exam.Subject)
                .Join(
                    databaseContext.UsersSubjects,
                    exam => exam.SubjectId,
                    userSubject => userSubject.SubjectId,
                    (exam, userSubject) => new { exam, userSubject }
                )
                .Where(selection => selection.userSubject.UserId == userId
                                 && selection.userSubject.SubjectRelation == subjectRelationType
                                 && (subjectRelationType == SubjectRelationType.TEACHING
                                    || selection.exam.StartTime <= now && selection.exam.EndTime >= now))
                .Select(selection => selection.exam)
                .OrderByDescending(exam => exam.StartTime)
                .Select(exam => ExamDTO.FromEntity(exam))
                .ToPagedList(page);
        }

        public IList<StudentExamDTO> GetStudentExams(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = GetExam(examId);

            CheckUserTeachesSubject(userId, exam.SubjectId);

            return GetStudentExamsQuery(examId)
                .Select(studentExam => StudentExamDTO.FromEntity(studentExam))
                .ToList();
        }

        public StudentExamDTO GetStudentExam(string token, int examId, int studentId)
        {
            int userId = userService.GetUserIdFromToken(token);

            return StudentExamDTO.FromEntity(GetStudentExam(userId, examId, studentId));
        }

        public IList<FileInfoDTO> GetStudentExamFileTree(string token, int examId, int studentId, string fileTreePath)
        {
            int userId = userService.GetUserIdFromToken(token);

            StudentExam studentExam = GetStudentExam(userId, examId, studentId);

            return fileService.GetFileTree(fileTreePath, studentExam);
        }

        public FileDTO GetStudentExamFileContent(string token, int examId, int studentId, string fileTreePath)
        {
            int userId = userService.GetUserIdFromToken(token);

            StudentExam studentExam = GetStudentExam(userId, examId, studentId);

            return fileService.GetFileContent(fileTreePath, studentExam);
        }

        public PhysicalFileResult GetStudentExamFile(string token, int examId, int studentId)
        {
            int userId = userService.GetUserIdFromToken(token);

            StudentExam studentExam = GetStudentExam(userId, examId, studentId);

            return fileService.GetFile(studentExam);
        }

        private StudentExam GetStudentExam(int userId, int examId, int studentId)
        {
            Exam exam = GetExam(examId);

            CheckUserTeachesSubject(userId, exam.SubjectId);

            StudentExam studentExam = GetStudentExamsQuery(examId)
                .Where(studentExam => studentExam.StudentId == studentId)
                .FirstOrDefault();

            if (studentExam == null)
            {
                throw new BadRequestException($"Exam with id '{examId}' for student with id '{studentId}' is not found");
            }

            return studentExam;
        }

        public IQueryable<StudentExam> GetStudentExamsQuery(int examId)
        {
            return databaseContext
                .StudentsExams
                .Include(studentExam => studentExam.Student)
                .ThenInclude(student => student.User)
                .Where(studentExam => studentExam.ExamId == examId);
        }

        public void CheckUserAttendsSubject(int userId, int subjectId)
        {
            UserSubject userSubject = GetUserSubject(userId, subjectId, SubjectRelationType.ATTENDING);

            if (userSubject == null)
            {
                throw new BadRequestException($"Authenticated user doesn't attend subject with id '{subjectId}'");
            }
        }

        public void CheckUserTeachesSubject(int userId, int subjectId)
        {
            UserSubject userSubject = GetUserSubject(userId, subjectId, SubjectRelationType.TEACHING);

            if (userSubject == null)
            {
                throw new BadRequestException($"Authenticated user doesn't teach subject with id '{subjectId}'");
            }
        }

        private UserSubject GetUserSubject(int userId, int subjectId, SubjectRelationType subjectRelationType)
        {
            return databaseContext
               .UsersSubjects
               .Where(userSubject => userSubject.UserId == userId
                                  && userSubject.SubjectId == subjectId
                                  && userSubject.SubjectRelation == subjectRelationType)
               .FirstOrDefault();
        }
    }
}
