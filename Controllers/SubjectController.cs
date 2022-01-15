using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Services.Interfaces;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ExamSaver.Controllers
{
    [ApiController]
    [Route("subjects")]
    [Authorize(Roles = RoleType.PROFESSOR)]
    public class SubjectController : ControllerBase
    {
        private readonly ISubjectService subjectService;
        private readonly IUserService userService;

        public SubjectController(ISubjectService subjectService, IUserService userService)
        {
            this.subjectService = subjectService;
            this.userService = userService;
        }

        [HttpGet]
        public IList<SubjectDTO> GetTeachingSubjects()
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return subjectService.GetTeachingSubjects(userId);
        }
    }
}
