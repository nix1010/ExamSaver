using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Services;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace ExamSaver.Controllers
{
    [ApiController]
    [Route("subjects")]
    [Authorize(Roles = RoleType.PROFESSOR)]
    public class SubjectController : ControllerBase
    {
        private readonly SubjectService subjectService;

        public SubjectController(SubjectService subjectService)
        {
            this.subjectService = subjectService;
        }

        [HttpGet]
        public IList<SubjectDTO> GetTeachingSubjects()
        {
            return subjectService.GetTeachingSubjects(Util.GetJWTToken(Request.Headers));
        }
    }
}
