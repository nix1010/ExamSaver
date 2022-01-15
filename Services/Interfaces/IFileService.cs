using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Services.Interfaces
{
    public interface IFileService
    {
        public IList<FileInfoDTO> GetFileTree(string fileTreePath, StudentExam studentExam);

        public FileDTO GetFileContent(string fileTreePath, StudentExam studentExam);

        public PhysicalFileResult GetFile(StudentExam studentExam);

        public string ExtractZipArchive(StudentExam studentExam);

        public string GetStudentExamFileExtractedDirectoryPath(StudentExam studentExam);

        public string SaveFile(IFormFile file, Exam exam, Student student);

        public void DeleteStudentExamFile(StudentExam studentExam);

        public string GetFileExtension(IFormFile file);

        public void DeleteDirectoryAndContents(string directoryPath);
    }
}
