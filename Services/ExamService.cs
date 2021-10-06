using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models.Entity;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ExamSaver.Services
{
    public class ExamService
    {
        private DatabaseContext databaseContext;
        private UserService userService;

        public ExamService(DatabaseContext databaseContext, UserService userService)
        {
            this.databaseContext = databaseContext;
            this.userService = userService;
        }

        public void SaveWork(IFormCollection form, int examId, string token)
        {
            if (form.Files.Count == 0 || form.Files.Count > 1)
            {
                throw new BadRequestException("One file must be provided");
            }

            foreach (IFormFile file in form.Files)
            {
                string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
                string fileExtension = Path.GetExtension(fileName);

                if (fileExtension != ".zip")
                {
                    throw new BadRequestException("Only files with '.zip' extension are allowed");
                }

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

                string resourcesPath = Path.Combine(Directory.GetCurrentDirectory(), Constant.EXAMS_DIRECTORY_PATH);
                string studentResourceIdentifier = Util.GetStudentResourceIdentifier(student, examId);
                string studentExamPath = Path.Combine(resourcesPath, studentResourceIdentifier);

                if (!Directory.Exists(studentExamPath))
                {
                    Directory.CreateDirectory(studentExamPath);
                }

                string filePath = Path.Combine(studentExamPath, $"{studentResourceIdentifier}.zip");

                using FileStream fileStream = new FileStream(filePath, FileMode.Create);
                file.CopyTo(fileStream);
            }
        }
    }
}
