﻿using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ExamSaver.Services
{
    public class FileService
    {
        private readonly AppSettings appSettings;

        public FileService(IOptions<AppSettings> optionsMonitor)
        {
            appSettings = optionsMonitor.Value;
        }

        public IList<FileInfoDTO> GetFileTree(string fileTreePath, StudentExam studentExam)
        {
            string studentExamFilePath = GetStudentExamFilePath(studentExam);

            CheckFileExists(studentExamFilePath);

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

        public FileDTO GetFileContent(string fileTreePath, StudentExam studentExam)
        {
            string studentExamFilePath = GetStudentExamFilePath(studentExam);

            using ZipArchive zipArchive = ZipFile.Open(studentExamFilePath, ZipArchiveMode.Read);

            ZipArchiveEntry zipArchiveEntry = zipArchive.GetEntry(fileTreePath);

            if (zipArchiveEntry == null || IsDirectory(zipArchiveEntry))
            {
                throw new NotFoundException("Requested file is not found");
            }

            using StreamReader streamReader = new StreamReader(zipArchiveEntry.Open());

            return new FileContentDTO()
            {
                Name = zipArchiveEntry.Name,
                Content = streamReader.ReadToEnd(),
                Size = zipArchiveEntry.Length
            };
        }

        public PhysicalFileResult GetFile(StudentExam studentExam)
        {
            string studentExamFilePath = GetStudentExamFilePath(studentExam);

            CheckFileExists(studentExamFilePath);

            return new PhysicalFileResult(Path.GetFullPath(studentExamFilePath), "application/octet-stream")
            {
                FileDownloadName = Path.GetFileName(studentExamFilePath)
            };
        }

        public string ExtractZipArchive(StudentExam studentExam)
        {
            string studentExamFilePath = GetStudentExamFilePath(studentExam);
            string studentExamFileExtractedDirectoryPath = GetStudentExamFileExtractedDirectoryPath(studentExam);

            CheckFileExists(studentExamFilePath);

            DeleteDirectoryAndContents(studentExamFileExtractedDirectoryPath);

            ZipFile.ExtractToDirectory(studentExamFilePath, studentExamFileExtractedDirectoryPath, true);

            return studentExamFileExtractedDirectoryPath;
        }

        public string GetStudentExamFileExtractedDirectoryPath(StudentExam studentExam)
        {
            string studentResourceIdentifier = Util.GetStudentExamResourceIdentifier(studentExam.Student, studentExam.ExamId);
            string studentExamDirectoryPath = Path.Combine(appSettings.ExamsDirectoryPath, studentResourceIdentifier);
            string studentExamFileExtractedDirectoryPath = Path.Combine(studentExamDirectoryPath, studentResourceIdentifier);

            return studentExamFileExtractedDirectoryPath;
        }

        private void CheckFileExists(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new NotFoundException("Student resource file is not found on the disk");
            }
        }

        private string GetStudentExamFilePath(StudentExam studentExam)
        {
            string studentResourceIdentifier = Util.GetStudentExamResourceIdentifier(studentExam.Student, studentExam.ExamId);
            string studentExamDirectoryPath = Path.Combine(appSettings.ExamsDirectoryPath, studentResourceIdentifier);
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
            string studentResourceIdentifier = Util.GetStudentExamResourceIdentifier(student, exam.Id);
            string studentExamDirectoryPath = Path.Combine(appSettings.ExamsDirectoryPath, studentResourceIdentifier);

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

        public void DeleteStudentExamFile(StudentExam studentExam)
        {
            string studentResourceIdentifier = Util.GetStudentExamResourceIdentifier(studentExam.Student, studentExam.ExamId);
            string studentExamDirectoryPath = Path.Combine(appSettings.ExamsDirectoryPath, studentResourceIdentifier);

            DeleteDirectoryAndContents(studentExamDirectoryPath);
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

        public void DeleteDirectoryAndContents(string directoryPath)
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }
}
