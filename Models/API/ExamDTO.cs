using ExamSaver.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class ExamDTO
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int SubjectId { get; set; }
        public string SubjectName { get; set; }

        public static ExamDTO FromEntity(Exam exam)
        {
            return new ExamDTO()
            {
                Id = exam.Id,
                StartTime = exam.StartTime,
                EndTime = exam.EndTime,
                SubjectId = exam.SubjectId,
                SubjectName = exam.Subject.Name
            };
        }
    }
}
