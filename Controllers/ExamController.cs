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

        [Route("create")]
        [HttpPost]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult CreateExam([FromBody] ExamDTO examDTO)
        {
            examService.CreateExam(Util.GetJWTToken(Request.Headers), examDTO);

            return Created(string.Empty, null);
        }

        [Route("{examId}/update")]
        [HttpPut]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult UpdateExam([FromBody] ExamDTO examDTO, [FromRoute] int examId)
        {
            examService.UpdateExam(Util.GetJWTToken(Request.Headers), examDTO, examId);

            return NoContent();
        }

        [Route("{examId}/submit")]
        [HttpPost]
        [Authorize(Roles = RoleType.STUDENT)]
        public IActionResult UploadExam([FromForm] IFormCollection form, [FromRoute] int examId)
        {
            examService.SubmitExam(Util.GetJWTToken(Request.Headers), form, examId);

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
        public IList<StudentExamDTO> GetExamStudents([FromRoute] int examId)
        {
            return examService.GetExamStudents(Util.GetJWTToken(Request.Headers), examId);
        }

        [Route("{examId}/students/{studentId}/tree/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<FileInfoDTO> GetStudentExamFileStructure([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            return examService.GetStudentExamFileTree(Util.GetJWTToken(Request.Headers), examId, studentId, fileTreePath);
        }

        [Route("{examId}/students/{studentId}/file/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public FileDTO GetStudentExamFile([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            return examService.GetStudentExamFile(Util.GetJWTToken(Request.Headers), examId, studentId, fileTreePath);
        }
    }
}