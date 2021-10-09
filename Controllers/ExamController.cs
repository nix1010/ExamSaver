using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ExamSaver.Configs;
using ExamSaver.Exceptions;
using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Services;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace ExamSaver.Controllers
{
    [ApiController]
    [Route("exams")]
    public class ExamController : ControllerBase
    {
        private readonly ExamService examService;

        public ExamController(ExamService examService)
        {
            this.examService = examService;
        }

        [HttpPost]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult SetExam([FromBody] ExamDTO examDTO)
        {
            examService.SetExam(Util.GetJWTToken(Request.Headers), examDTO);

            return Created(string.Empty, null);
        }

        [Route("{examId}")]
        [HttpPost]
        [Authorize(Roles = RoleType.STUDENT)]
        public IActionResult UploadExam([FromForm] IFormCollection form, [FromRoute] int examId)
        {
            examService.SaveExam(form, examId, Util.GetJWTToken(Request.Headers));

            return Created(string.Empty, null);
        }

        [Route("taking")]
        [HttpGet]
        [Authorize(Roles = RoleType.STUDENT)]
        public IList<ExamDTO> GetTakingExams()
        {
            return examService.GetTakingExams(Util.GetJWTToken(Request.Headers));
        }

        [Route("holding")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<ExamDTO> GetHoldingExams()
        {
            return examService.GetHoldingExams(Util.GetJWTToken(Request.Headers));
        }

        [Route("{examId}/students")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<StudentExamDTO> GetStudentExams([FromRoute] int examId)
        {
            return examService.GetStudentExams(Util.GetJWTToken(Request.Headers), examId);
        }

        [Route("{examId}/students/{studentId}/{**url}")]
        [HttpGet]
        public string GetStudentExam([FromRoute] int examId, [FromRoute] int studentId)
        {
            return "OK";
        }
    }
}