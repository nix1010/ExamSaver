using ExamSaver.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Services.Interfaces
{
    public interface ISubjectService
    {
        public IList<SubjectDTO> GetTeachingSubjects(int userId);
    }
}
