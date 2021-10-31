using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using ExamSaver.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExamSaver.Services
{
    public class MossService
    {
        private readonly ExamService examService;
        private readonly FileService fileService;
        private readonly UserService userService;
        private readonly DatabaseContext databaseContext;

        public MossService(ExamService examService, FileService fileService, UserService userService, DatabaseContext databaseContext)
        {
            this.examService = examService;
            this.fileService = fileService;
            this.userService = userService;
            this.databaseContext = databaseContext;
        }

        public IList<MossResultDTO> GetMossResults(string token, int examId)
        {
            return GetMossResultsQuery(token, examId)
                .OrderByDescending(mossResult => mossResult.Submitted)
                .Select(mossResult => MossResultDTO.FromEntity(mossResult))
                .ToList();
        }

        public void DeleteMossResult(string token, int examId, int mossResultId)
        {
            MossResult mossResult = GetMossResultsQuery(token, examId)
                .Where(mossResult => mossResult.Id == mossResultId)
                .FirstOrDefault();

            if (mossResult == null)
            {
                throw new NotFoundException("Similarity result is not found");
            }

            databaseContext.MossResults.Remove(mossResult);

            databaseContext.SaveChanges();
        }

        private IQueryable<MossResult> GetMossResultsQuery(string token, int examId)
        {
            int userId = userService.GetUserIdFromToken(token);

            Exam exam = examService.GetExamEntity(token, examId, SubjectRelationType.TEACHING);

            examService.CheckUserTeachesSubject(userId, exam.SubjectId);

            return databaseContext.MossResults
                .Where(mossResult => mossResult.ExamId == examId);
        }

        public MossRunResultDTO PerformMoss(string token, int examId, MossRequestDTO mossRequestDTO)
        {
            string language = GetLanguageFromExtension(mossRequestDTO.FileExtension);
            List<StudentExam> examStudents = examService.GetExamStudentsQuery(token, examId).ToList();

            if (examStudents.Count < 2)
            {
                throw new BadRequestException("Required minimum 2 students for the similarity check");
            }

            StringBuilder runMessageBuilder = new StringBuilder();
            string mossFilePath = Path.Combine(Directory.GetCurrentDirectory(), Constant.MOSS_RELATIVE_FILE_PATH);
            StringBuilder argumentBuilder = new StringBuilder($"perl \"{mossFilePath}\" -m 1000000 -l {language} -d");

            AddComment(argumentBuilder, mossRequestDTO.Comment);

            int studentFilePathsSetCount = 0;
            try
            {
                foreach (StudentExam studentExam in examStudents)
                {
                    string studentExamFileExtractedDirectoryPath = fileService.ExtractZipArchive(studentExam);

                    try
                    {
                        SetFilePaths(argumentBuilder, studentExamFileExtractedDirectoryPath, mossRequestDTO.FileExtension);
                        ++studentFilePathsSetCount;
                    }
                    catch (NotFoundException)
                    {
                        AppendStudentToRunMessage(runMessageBuilder, studentExam);
                    }
                }

                if (studentFilePathsSetCount < 2)
                {
                    throw new BadRequestException($"Found files for {studentFilePathsSetCount} student(s), at least 2 students are required");
                }

                DateTime submitDateTime = DateTime.Now;

                string resultUrl = RunMoss(argumentBuilder);

                databaseContext.MossResults.Add(new MossResult()
                {
                    ExamId = examId,
                    Url = resultUrl,
                    Comment = mossRequestDTO.Comment,
                    Submitted = submitDateTime
                });

                databaseContext.SaveChanges();

                return new MossRunResultDTO()
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
            }
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
                int examsDirectoryStartIndex = filePath.IndexOf(Constant.EXAMS_RELATIVE_DIRECTORY_PATH);
                string relativeFilePath = filePath;

                if (examsDirectoryStartIndex != -1)
                {
                    int studentExamResourceIndex = examsDirectoryStartIndex + Constant.EXAMS_RELATIVE_DIRECTORY_PATH.Length + 1;
                    relativeFilePath = filePath.Substring(studentExamResourceIndex);
                }

                argumentBuilder.Append($" \"{relativeFilePath}\"");
            }
        }

        private string RunMoss(StringBuilder argumentBuilder)
        {
            ProcessStartInfo processStartInfo = new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/C {argumentBuilder.Replace('\\', '/')}",
                WorkingDirectory = Constant.EXAMS_RELATIVE_DIRECTORY_PATH,
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

        private void AppendStudentToRunMessage(StringBuilder runMessageBuilder, StudentExam studentExam)
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
