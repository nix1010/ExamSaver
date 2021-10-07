using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models;
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

        public void SaveWork(IFormCollection form, int examId, string token)
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

            CheckRequestValidity(form, exam, student);

            foreach (IFormFile file in form.Files)
            {
                string resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), Constant.EXAMS_DIRECTORY_PATH);
                string studentResourceIdentifier = Util.GetStudentResourceIdentifier(student, examId);
                string studentExamPath = Path.Combine(resourcesPath, studentResourceIdentifier);

                if (!Directory.Exists(studentExamPath))
                {
                    Directory.CreateDirectory(studentExamPath);
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

        private void CheckRequestValidity(IFormCollection form, Exam exam, Student student)
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

        private void ExtractZipFile(string filePath)
        {
            ZipArchive zipArchive = ZipFile.Open(filePath, ZipArchiveMode.Read);

            foreach (ZipArchiveEntry zipArchiveEntry in zipArchive.Entries)
            {
                
            }
        }
    }
}
