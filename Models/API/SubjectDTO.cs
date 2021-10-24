using ExamSaver.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class SubjectDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public static SubjectDTO FromEntity(Subject subject)
        {
            return new SubjectDTO()
            {
                Id = subject.Id,
                Name = subject.Name
            };
        }
    }
}
