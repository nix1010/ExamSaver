using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace ExamSaver.Services
{
    public class SimilarityService
    {
        private static readonly ConcurrentDictionary<int, object> similarityCheckLocks = new ConcurrentDictionary<int, object>();

        private readonly ExamService examService;
        private readonly FileService fileService;
        private readonly UserService userService;
        private readonly DatabaseContext databaseContext;
        private readonly AppSettings appSettings;

        public SimilarityService(ExamService examService, FileService fileService, UserService userService, DatabaseContext databaseContext, IOptions<AppSettings> optionsMonitor)
        {
            this.examService = examService;
            this.fileService = fileService;
            this.userService = userService;
            this.databaseContext = databaseContext;
            this.appSettings = optionsMonitor.Value;
        }

        public IList<SimilarityResultDTO> GetSimilarityResults(string token, int examId)
        {
            CheckRequestValidity(token, examId);

            return GetSimilarityResultsQuery(examId)
                .OrderByDescending(similarityResult => similarityResult.Submitted)
                .Select(similarityResult => SimilarityResultDTO.FromEntity(similarityResult))
                .ToList();
        }

        public void DeleteSimilarityResult(string token, int examId, int similarityResultId)
        {
            CheckRequestValidity(token, examId);

            SimilarityResult similarityResult = GetSimilarityResultsQuery(examId)
                .Where(similarityResult => similarityResult.Id == similarityResultId)
                .FirstOrDefault();

            if (similarityResult == null)
            {
                throw new NotFoundException($"Similarity result with id '{similarityResultId}' is not found");
            }

            databaseContext.SimilarityResults.Remove(similarityResult);

            databaseContext.SaveChanges();
        }

        private IQueryable<SimilarityResult> GetSimilarityResultsQuery(int examId)
        {
            return databaseContext
                .SimilarityResults
                .Where(similarityResult => similarityResult.ExamId == examId);
        }

        public SimilarityRunResultDTO PerformSimilarityCheck(string token, int examId, SimilarityRequestDTO similarityRequestDTO)
        {
            CheckRequestValidity(token, examId);

            string language = GetLanguageFromExtension(similarityRequestDTO.FileExtension);

            List<StudentExam> examStudents = examService.GetStudentExamsQuery(examId).ToList();

            if (examStudents.Count < 2)
            {
                throw new BadRequestException("Required minimum 2 students for similarity check");
            }

            StringBuilder runMessageBuilder = new StringBuilder();
            string mossFilePath = Path.Combine(Directory.GetCurrentDirectory(), Constant.MOSS_RELATIVE_FILE_PATH);
            StringBuilder argumentBuilder = new StringBuilder($"perl \"{mossFilePath}\" -m 1000000 -l {language} -d");

            AddComment(argumentBuilder, similarityRequestDTO.Comment);

            int studentFilePathsSetCount = 0;

            object similarityCheckLock = similarityCheckLocks.GetOrAdd(examId, new object());

            lock (similarityCheckLock)
            {
                try
                {
                    foreach (StudentExam studentExam in examStudents)
                    {
                        string studentExamFileExtractedDirectoryPath = fileService.ExtractZipArchive(studentExam);

                        try
                        {
                            SetFilePaths(argumentBuilder, studentExamFileExtractedDirectoryPath, similarityRequestDTO.FileExtension);
                            ++studentFilePathsSetCount;
                        }
                        catch (NotFoundException)
                        {
                            AppendStudentFilesErrorMessage(runMessageBuilder, studentExam);
                        }
                    }

                    if (studentFilePathsSetCount < 2)
                    {
                        throw new BadRequestException($"Found files for {studentFilePathsSetCount} student(s), at least 2 students are required");
                    }

                    DateTime submitDateTime = DateTime.Now;

                    string resultUrl = RunSimilarityCheck(argumentBuilder);

                    databaseContext.SimilarityResults.Add(new SimilarityResult()
                    {
                        ExamId = examId,
                        Url = resultUrl,
                        Comment = similarityRequestDTO.Comment,
                        Submitted = submitDateTime
                    });

                    databaseContext.SaveChanges();

                    return new SimilarityRunResultDTO()
                    {
                        RunMessage = runMessageBuilder.Length > 0 ? runMessageBuilder.ToString() : null
                    };
                }
                finally
                {
                    foreach (StudentExam studentExam in examStudents)
                    {
                        fileService.DeleteDirectoryAndContents(fileService.GetStudentExamFileExtractedDirectoryPath(studentExam));
                    }

                    similarityCheckLocks.TryRemove(examId, out object _);
                }
            }
        }

        private void CheckRequestValidity(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = examService.GetExam(examId);

            examService.CheckUserTeachesSubject(userId, exam.SubjectId);
        }

        private void SetFilePaths(StringBuilder argumentBuilder, string studentExamFileExtractedDirectoryPath, string fileExtension)
        {
            string[] filePaths = Directory.GetFiles(studentExamFileExtractedDirectoryPath, $"*.{fileExtension}", SearchOption.AllDirectories);

            if (filePaths.Length == 0)
            {
                throw new NotFoundException("Files not found");
            }

            foreach (string filePath in filePaths)
            {
                int examsDirectoryStartIndex = filePath.IndexOf(appSettings.ExamsDirectoryPath);
                string relativeFilePath = filePath;

                if (examsDirectoryStartIndex != -1)
                {
                    int studentExamResourceIndex = examsDirectoryStartIndex + appSettings.ExamsDirectoryPath.Length + 1;
                    relativeFilePath = filePath.Substring(studentExamResourceIndex);
                }

                argumentBuilder.Append($" \"{relativeFilePath}\"");
            }
        }

        private string RunSimilarityCheck(StringBuilder argumentBuilder)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/C {argumentBuilder.Replace('\\', '/')}",
                WorkingDirectory = appSettings.ExamsDirectoryPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            Process process = Process.Start(processStartInfo);

            string stdOutput = process.StandardOutput.ReadToEnd().Trim();
            string stdErrorOutput = process.StandardError.ReadToEnd().Trim();

            if (process.ExitCode != 0)
            {
                throw new Exception($"Moss error: {stdErrorOutput}");
            }

            int resultUrlIndex = stdOutput.IndexOf("http");

            if (resultUrlIndex == -1)
            {
                throw new Exception("Url not provided");
            }

            return stdOutput.Substring(resultUrlIndex);
        }

        private string GetLanguageFromExtension(string fileExtension)
        {
            switch (fileExtension)
            {
                case "c":
                case "h":
                case "cpp":
                case "hpp": return "c";
                case "java": return "java";
                case "cs": return "csharp";
                case "py": return "python";
                default: throw new BadRequestException($"File extension '{fileExtension}' is not supported");
            }
        }

        private void AddComment(StringBuilder argumentBuilder, string comment)
        {
            if (comment != null)
            {
                comment = comment.Trim();

                if (comment != "")
                {
                    argumentBuilder.Append($" -c \"{comment}\"");
                }
            }
        }

        private void AppendStudentFilesErrorMessage(StringBuilder runMessageBuilder, StudentExam studentExam)
        {
            string firstName = studentExam.Student.User.FirstName;
            string lastName = studentExam.Student.User.LastName;
            string index = studentExam.Student.Index;

            if (runMessageBuilder.Length == 0)
            {
                runMessageBuilder.Append("Files not found for: ");
            }
            else
            {
                runMessageBuilder.Append(", ");
            }

            runMessageBuilder.Append($"{firstName} {lastName} {index}");
        }
    }
}
