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
    [Route("exams")]
    public class ExamController : ControllerBase
    {
        private readonly ExamService examService;

        public ExamController(ExamService examService)
        {
            this.examService = examService;
        }

        [Route("taking")]
        [HttpGet]
        [Authorize(Roles = RoleType.STUDENT)]
        public IList<ExamDTO> GetTakingExams()
        {
            return examService.GetTakingExams(Util.GetJWTToken(Request.Headers));
        }

        [Route("taking/{examId}")]
        [HttpGet]
        [Authorize]
        public ExamDTO GetTakingExam(int examId)
        {
            return examService.GetTakingExam(Util.GetJWTToken(Request.Headers), examId);
        }

        [Route("taking/{examId}")]
        [HttpPost]
        [Authorize(Roles = RoleType.STUDENT)]
        public IActionResult UploadExam([FromForm] IFormCollection form, [FromRoute] int examId)
        {
            examService.SubmitExam(Util.GetJWTToken(Request.Headers), form, examId);

            return Created(string.Empty, null);
        }

        [Route("holding")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<ExamDTO> GetHoldingExams()
        {
            return examService.GetHoldingExams(Util.GetJWTToken(Request.Headers));
        }

        [Route("holding/{examId}")]
        [HttpGet]
        [Authorize]
        public ExamDTO GetHoldingExam(int examId)
        {
            return examService.GetHoldingExam(Util.GetJWTToken(Request.Headers), examId);
        }

        [Route("holding")]
        [HttpPost]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult AddExam([FromBody] ExamDTO examDTO)
        {
            examService.AddExam(Util.GetJWTToken(Request.Headers), examDTO);

            return Created(string.Empty, null);
        }

        [Route("holding/{examId}")]
        [HttpPut]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult UpdateExam([FromBody] ExamDTO examDTO, [FromRoute] int examId)
        {
            examService.UpdateExam(Util.GetJWTToken(Request.Headers), examDTO, examId);

            return NoContent();
        }

        [Route("holding/{examId}/students")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<StudentExamDTO> GetExamStudents([FromRoute] int examId)
        {
            return examService.GetExamStudents(Util.GetJWTToken(Request.Headers), examId);
        }

        [Route("holding/{examId}/students/{studentId}/tree/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<FileInfoDTO> GetStudentExamFileStructure([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            return examService.GetStudentExamFileTree(Util.GetJWTToken(Request.Headers), examId, studentId, fileTreePath);
        }

        [Route("holding/{examId}/students/{studentId}/file/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public FileDTO GetStudentExamFile([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            return examService.GetStudentExamFile(Util.GetJWTToken(Request.Headers), examId, studentId, fileTreePath);
        }
    }
}