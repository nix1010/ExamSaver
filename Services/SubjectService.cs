using ExamSaver.Data;
using ExamSaver.Models;
using ExamSaver.Models.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Services
{
    public class SubjectService
    {
        private readonly DatabaseContext databaseContext;
        private readonly UserService userService;

        public SubjectService(DatabaseContext databaseContext, UserService userService)
        {
            this.databaseContext = databaseContext;
            this.userService = userService;
        }

        public IList<SubjectDTO> GetTeachingSubjects(string token)
        {
            int userId = userService.GetUserIdFromToken(token);

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
                    .Select(userSubject => new SubjectDTO()
                    {
                        Id = userSubject.SubjectId,
                        Name = userSubject.Subject.Name
                    })
                    .ToList();
        }
    }
}
