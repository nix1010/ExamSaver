using ExamSaver.Models;
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
    public interface IExamService
    {
        public void AddExam(int userId, ExamDTO examDTO);

        public void UpdateExam(int userId, int examId, ExamDTO examDTO);

        public void DeleteExam(int userId, int examId);

        public void SubmitWork(int userId, int examId, IFormFileCollection formFiles);

        public PagedList<ExamDTO> GetHoldingExams(int userId, int page);

        public ExamDTO GetHoldingExam(int userId, int examId);

        public PagedList<ExamDTO> GetTakingExams(int userId, int page);

        public ExamDTO GetTakingExam(int userId, int examId);

        public Exam GetExam(int examId);

        public IList<StudentExamDTO> GetStudentExams(int userId, int examId);

        public StudentExamDTO GetStudentExam(int userId, int examId, int studentId);

        public IList<FileInfoDTO> GetStudentExamFileTree(int userId, int examId, int studentId, string fileTreePath);

        public FileDTO GetStudentExamFileContent(int userId, int examId, int studentId, string fileTreePath);

        public PhysicalFileResult GetStudentExamFile(int userId, int examId, int studentId);

        public IQueryable<StudentExam> GetStudentExamsQuery(int examId);

        public void CheckUserAttendsSubject(int userId, int subjectId);

        public void CheckUserTeachesSubject(int userId, int subjectId);
    }
}
