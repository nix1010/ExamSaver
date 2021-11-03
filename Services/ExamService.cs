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

            CheckExamAddValidity(examDTO, userId);

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

            Exam exam = databaseContext
                .Exams
                .Find(examId);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' is not found");
            }

            CheckExamUpdateValidity(examDTO, userId);

            exam.StartTime = examDTO.StartTime;
            exam.EndTime = examDTO.EndTime;

            databaseContext.Update(exam);

            databaseContext.SaveChanges();
        }

        private void CheckExamAddValidity(ExamDTO examDTO, int userId)
        {
            CheckExamUpdateValidity(examDTO, userId);

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

        private void CheckExamUpdateValidity(ExamDTO examDTO, int userId)
        {
            CheckUserTeachesSubject(userId, examDTO.SubjectId);

            if (examDTO.StartTime >= examDTO.EndTime)
            {
                throw new BadRequestException("Starting time must be greater than ending time");
            }
        }

        public void SubmitExam(string token, int examId, IFormCollection form)
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

            Exam exam = databaseContext
                .Exams
                .FirstOrDefault(exam => exam.Id == examId);

            if (exam == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' is not found");
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

        public PagedList<ExamDTO> GetHoldingExams(string token, int page)
        {
            return GetExams(token, SubjectRelationType.TEACHING, page);
        }

        public ExamDTO GetHoldingExam(string token, int examId)
        {
            return GetExam(token, examId, SubjectRelationType.TEACHING);
        }

        public PagedList<ExamDTO> GetTakingExams(string token, int page)
        {
            return GetExams(token, SubjectRelationType.ATTENDING, page);
        }

        public ExamDTO GetTakingExam(string token, int examId)
        {
            return GetExam(token, examId, SubjectRelationType.ATTENDING);
        }

        public ExamDTO GetExam(string token, int examId, SubjectRelationType subjectRelationType)
        {
            return ExamDTO.FromEntity(GetExamEntity(token, examId, subjectRelationType));
        }

        public Exam GetExamEntity(string token, int examId, SubjectRelationType subjectRelationType)
        {
            Exam exam = GetExamsQuery(token, subjectRelationType)
                .Where(exam => exam.Id == examId)
                .FirstOrDefault();

            if (exam == null)
            {
                throw new NotFoundException($"Exam with id '{examId}' is not found for the authenticated user");
            }

            return exam;
        }

        public PagedList<ExamDTO> GetExams(string token, SubjectRelationType subjectRelationType, int page = 1)
        {
            return GetExamsQuery(token, subjectRelationType)
                .OrderByDescending(exam => exam.StartTime)
                .Select(exam => ExamDTO.FromEntity(exam))
                .ToPagedList(page);
        }

        public IQueryable<Exam> GetExamsQuery(string token, SubjectRelationType subjectRelationType)
        {
            int userId = userService.GetUserIdFromToken(token);
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
                .Select(selection => selection.exam);
        }

        public IList<StudentExamDTO> GetExamStudents(string token, int examId)
        {
            return GetExamStudentsQuery(token, examId)
                .Select(studentExam => StudentExamDTO.FromEntity(studentExam))
                .ToList();
        }

        public StudentExamDTO GetStudentExam(string token, int examId, int studentId)
        {
            return StudentExamDTO.FromEntity(GetStudentExamEntity(token, examId, studentId));
        }

        public IList<FileInfoDTO> GetStudentExamFileTree(string token, int examId, int studentId, string fileTreePath)
        {
            StudentExam studentExam = GetStudentExamEntity(token, examId, studentId);

            return fileService.GetFileTree(fileTreePath, studentExam);
        }

        public FileDTO GetStudentExamFileContent(string token, int examId, int studentId, string fileTreePath)
        {
            StudentExam studentExam = GetStudentExamEntity(token, examId, studentId);

            return fileService.GetFileContent(fileTreePath, studentExam);
        }

        public PhysicalFileResult GetStudentExamFile(string token, int examId, int studentId)
        {
            StudentExam studentExam = GetStudentExamEntity(token, examId, studentId);

            return fileService.GetFile(studentExam);
        }

        private StudentExam GetStudentExamEntity(string token, int examId, int studentId)
        {
            StudentExam studentExam = GetExamStudentsQuery(token, examId)
                .Where(studentExam => studentExam.StudentId == studentId)
                .FirstOrDefault();

            if (studentExam == null)
            {
                throw new BadRequestException($"Exam with id '{examId}' for student with id '{studentId}' is not found");
            }

            return studentExam;
        }

        public IQueryable<StudentExam> GetExamStudentsQuery(string token, int examId)
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
                .Where(studentExam => studentExam.ExamId == examId);
        }

        public void CheckUserTeachesSubject(int userId, int subjectId)
        {
            UserSubject userSubject = databaseContext
               .UsersSubjects
               .Where(userSubject => userSubject.UserId == userId
                                  && userSubject.SubjectId == subjectId
                                  && userSubject.SubjectRelation == SubjectRelationType.TEACHING)
               .FirstOrDefault();

            if (userSubject == null)
            {
                throw new BadRequestException($"Authenticated user doesn't teach subject with id '{subjectId}'");
            }
        }
    }
}
