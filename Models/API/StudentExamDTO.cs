using ExamSaver.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class StudentExamDTO
    {
        public int StudentId { get; set; }
        public int ExamId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Index { get; set; }
        public DateTime UploadTime { get; set; }
        public string ExamFilePath { get; set; }

        public static StudentExamDTO FromEntity(StudentExam studentExam)
        {
            return new StudentExamDTO()
            {
                StudentId = studentExam.StudentId,
                ExamId = studentExam.ExamId,
                FirstName = studentExam.Student.User.FirstName,
                LastName = studentExam.Student.User.LastName,
                Index = studentExam.Student.Index,
                UploadTime = studentExam.UploadTime,
                ExamFilePath = studentExam.ExamFilePath
            };
        }
    }
}
