using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using ExamSaver.Services;
using ExamSaver.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using Xunit;

namespace ExamSaver.UnitTests
{
    public class ExamServiceTest
    {
        private readonly DatabaseContext databaseContext;
        private readonly IExamService examService;
        private readonly Mock<IFileService> mockFileService;
        private readonly string mockFilePath = "path/to/file";

        public ExamServiceTest()
        {
            databaseContext = new DatabaseContext();

            mockFileService = new Mock<IFileService>();
            mockFileService.Setup(fs => fs.DeleteStudentExamFile(It.IsAny<StudentExam>())).Callback(() => { });
            mockFileService.Setup(fs => fs.SaveFile(It.IsAny<IFormFile>(), It.IsAny<Exam>(), It.IsAny<Student>())).Returns(mockFilePath);

            examService = new ExamService(databaseContext, mockFileService.Object);

            SeedData();
        }

        private void SeedData()
        {
            databaseContext.Database.EnsureDeleted();
            databaseContext.Database.EnsureCreated();

            DatabaseInitializer.PopulateDatabase(databaseContext);
        }

        [Fact]
        public void AddExam_NewExam_ShouldAddExam()
        {
            ExamDTO examDTO = new ExamDTO()
            {
                SubjectId = 2,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };

            int userId = 1;

            examService.AddExam(userId, examDTO);
        }

        [Fact]
        public void AddExam_ActiveExam_ShouldThrowException()
        {
            ExamDTO examDTO = new ExamDTO()
            {
                SubjectId = 1,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };

            int userId = 1;

            Assert.Throws<BadRequestException>(() => examService.AddExam(userId, examDTO));
        }

        [Fact]
        public void AddExam_NonTeachingSubject_ShouldThrowException()
        {
            ExamDTO examDTO = new ExamDTO()
            {
                SubjectId = 3,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(2)
            };

            int userId = 1;

            Assert.Throws<BadRequestException>(() => examService.AddExam(userId, examDTO));
        }

        [Fact]
        public void UpdateExam_BadStartAndEndTime_ShouldThrowException()
        {
            ExamDTO examDTO = new ExamDTO()
            {
                SubjectId = 3,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddHours(-2)
            };

            int userId = 1;

            Assert.Throws<BadRequestException>(() => examService.AddExam(userId, examDTO));
        }

        [Fact]
        public void DeleteExam_DeletesExamWithSpecifiedId_ShouldDeleteExam()
        {
            int userId = 1;
            int examId = 1;

            examService.DeleteExam(userId, examId);
        }

        [Fact]
        public void GetExam_GetNonExistingExam_ShouldThrowException()
        {
            int examId = 10;

            Assert.Throws<NotFoundException>(() => examService.GetExam(examId));
        }

        [Fact]
        public void GetHoldingExam_GetExamForTeachingSubject_ShouldReturnExam()
        {
            int userId = 1;
            int examId = 1;

            ExamDTO examDTO = examService.GetHoldingExam(userId, examId);

            Assert.NotNull(examDTO);
            Assert.Equal(examDTO.Id, examId);
        }

        [Fact]
        public void GetHoldingExam_GetExamForNonTeachingSubject_ShouldThrowException()
        {
            int userId = 1;
            int examId = 4;

            Assert.Throws<BadRequestException>(() => examService.GetHoldingExam(userId, examId));
        }

        [Fact]
        public void SubmitWork_SubmitExamMockZipFileToActiveExam_ShouldSubmitExamWork()
        {
            int examId = 3;
            int userId = 1;
            int studentId = 3;

            FormFileCollection formFiles = CreateMockFile();

            examService.SubmitWork(studentId, examId, formFiles);

            StudentExamDTO studentExamDTO = examService.GetStudentExam(userId, examId, studentId);

            Assert.NotNull(studentExamDTO);
            Assert.Equal(studentExamDTO.ExamFilePath, mockFilePath);
        }

        [Fact]
        public void SubmitWork_SubmitExamMockZipFileToNotActiveExam_ShouldThrowException()
        {
            int examId = 2;
            int studentId = 3;

            FormFileCollection formFiles = CreateMockFile();

            Assert.Throws<BadRequestException>(() => examService.SubmitWork(studentId, examId, formFiles));
        }

        private FormFileCollection CreateMockFile()
        {
            return new FormFileCollection()
            {
                new FormFile(null, 0, 0, "Mock file", "Mock file name")
            };
        }
    }
}
