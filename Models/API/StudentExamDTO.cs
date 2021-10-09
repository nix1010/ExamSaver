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
        public string ExamPath { get; set; }
    }
}
