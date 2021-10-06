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

        [HttpGet]
        public string Test()
        {
            return "OK";
        }

        [Authorize(Roles = RoleType.STUDENT)]
        [HttpPost]
        [Route("{examId}")]
        public IActionResult UploadWork([FromForm] IFormCollection form, [FromRoute] int examId)
        {
            examService.SaveWork(form, examId, Util.GetJWTToken(Request.Headers));

            return Created(string.Empty, null);
        }
    }
}