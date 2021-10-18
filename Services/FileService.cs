using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ExamSaver.Services
{
    public class FileService
    {
        public IList<FileInfoDTO> GetFileTree(string fileTreePath, StudentExam studentExam)
        {
            string studentExamFilePath = GetStudentExamFilePath(studentExam);

            if (!File.Exists(studentExamFilePath))
            {
                throw new NotFoundException("Student resource file is not found on the disk");
            }

            using ZipArchive zipArchive = ZipFile.Open(studentExamFilePath, ZipArchiveMode.Read);

            if (!fileTreePath.Equals(string.Empty))
            {
                if (!fileTreePath.EndsWith(Path.AltDirectorySeparatorChar))
                {
                    fileTreePath += Path.AltDirectorySeparatorChar;
                }
            
                ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(fileTreePath);

                if (zipArchiveEntry == null || !IsDirectory(zipArchiveEntry))
                {
                    throw new NotFoundException("Requested directory is not found");
                }
            }

            return zipArchive
                .Entries
                .Where(fileStructure => IsOnLevel(fileStructure, fileTreePath))
                .Select(zipArchiveEntry =>
                {
                    bool isDirectory = IsDirectory(zipArchiveEntry);

                    return new FileInfoDTO()
                    {
                        IsDirectory = isDirectory,
                        Name = isDirectory ? new DirectoryInfo(zipArchiveEntry.FullName).Name : zipArchiveEntry.Name,
                        FullPath = zipArchiveEntry.FullName,
                        Size = zipArchiveEntry.Length
                    };
                })
                .OrderByDescending(fileInfo => fileInfo.IsDirectory)
                .ThenBy(fileInfo => fileInfo.Name)
                .ToList();
        }

        public FileDTO GetFile(string fileTreePath, StudentExam studentExam)
        {
            string studentExamFilePath = GetStudentExamFilePath(studentExam);

            using ZipArchive zipArchive = ZipFile.Open(studentExamFilePath, ZipArchiveMode.Read);

            ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(fileTreePath);

            if (zipArchiveEntry == null || IsDirectory(zipArchiveEntry))
            {
                throw new NotFoundException("Requested file is not found");
            }

            using StreamReader streamReader = new StreamReader(zipArchiveEntry.Open());

            return new FileDTO()
            {
                Name = zipArchiveEntry.Name,
                Content = streamReader.ReadToEnd()
            };
        }

        public string GetStudentExamFilePath(StudentExam studentExam)
        {
            string studentExamDirectoryPath = Util.GetStudentExamDirectoryPath(studentExam.Student, studentExam.ExamId);
            string studentResourceIdentifier = Util.GetStudentResourceIdentifier(studentExam.Student, studentExam.ExamId);
            string studentExamFilePath = Path.Combine(studentExamDirectoryPath, $"{studentResourceIdentifier}.zip");

            return studentExamFilePath;
        }

        private bool IsOnLevel(ZipArchiveEntry zipArchiveEntry, string fileTreePath)
        {
            string entryName;

            if (IsDirectory(zipArchiveEntry))
            {
                entryName = new DirectoryInfo(zipArchiveEntry.FullName).Name + "/";
            }
            else
            {
                entryName = zipArchiveEntry.Name;
            }
            
            return fileTreePath + entryName == zipArchiveEntry.FullName;
        }

        private bool IsDirectory(ZipArchiveEntry zipArchiveEntry)
        {
            return Equals(zipArchiveEntry.Name, string.Empty) && zipArchiveEntry.FullName.EndsWith("/");
        }

        public void SaveFile(IFormFile file, Exam exam, Student student, out string examFilePath)
        {
            string studentExamDirectoryPath = Util.GetStudentExamDirectoryPath(student, exam.Id);
            string studentResourceIdentifier = Util.GetStudentResourceIdentifier(student, exam.Id);

            if (!Directory.Exists(studentExamDirectoryPath))
            {
                Directory.CreateDirectory(studentExamDirectoryPath);
            }
            else
            {
                DeleteExistingContent(studentExamDirectoryPath);
            }

            examFilePath = Path.Combine(studentExamDirectoryPath, $"{studentResourceIdentifier}.zip");

            if (Equals(GetFileExtension(file), ".zip"))
            {
                using FileStream fileStream = new FileStream(examFilePath, FileMode.Create);
                file.CopyTo(fileStream);
            }
            else
            {
                string temporaryDirectoryName = "temp";
                string temporaryDirectoryPath = Path.Combine(studentExamDirectoryPath, temporaryDirectoryName);
                string temporaryFilePath = Path.Combine(temporaryDirectoryPath, file.FileName);

                Directory.CreateDirectory(temporaryDirectoryPath);

                using (FileStream fileStream = new FileStream(temporaryFilePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                ZipFile.CreateFromDirectory(temporaryDirectoryPath, examFilePath);

                Directory.Delete(temporaryDirectoryPath, true);
            }
        }

        public string GetFileExtension(IFormFile file)
        {
            string fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition).FileName.Trim('"');
            return Path.GetExtension(fileName);
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
