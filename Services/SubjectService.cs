using ExamSaver.Data;
using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace ExamSaver.Services
{
    public class SubjectService : ISubjectService
    {
        private readonly DatabaseContext databaseContext;

        public SubjectService(DatabaseContext databaseContext)
        {
            this.databaseContext = databaseContext;
        }

        public IList<SubjectDTO> GetTeachingSubjects(int userId)
        {
            return databaseContext
                    .Subjects
                    .Join(
                        databaseContext.UsersSubjects,
                        subject => subject.Id,
                        userSubject => userSubject.SubjectId,
                        (subject, userSubject) => userSubject
                    )
                    .Where(userSubject => userSubject.SubjectRelation == SubjectRelationType.TEACHING
                                       && userSubject.UserId == userId)
                    .Select(userSubject => SubjectDTO.FromEntity(userSubject.Subject))
                    .ToList();
        }
    }
}
