using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Services;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.IO;

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
        public IList<ExamDTO> GetTakingExams([FromQuery] int? page)
        {
            PagedList<ExamDTO> exams = examService.GetTakingExams(Util.GetJWTToken(Request.Headers), Util.GetPage(page));

            Util.SetPageHeader(Response, exams.Page);
            
            return exams;
        }

        [Route("taking/{examId}")]
        [HttpGet]
        [Authorize]
        public ExamDTO GetTakingExam([FromRoute] int examId)
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
        public IList<ExamDTO> GetHoldingExams([FromQuery] int? page)
        {
            PagedList<ExamDTO> exams = examService.GetHoldingExams(Util.GetJWTToken(Request.Headers), Util.GetPage(page));

            Util.SetPageHeader(Response, exams.Page);

            return exams;
        }

        [Route("holding/{examId}")]
        [HttpGet]
        [Authorize]
        public ExamDTO GetHoldingExam([FromRoute] int examId)
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

        [Route("holding/{examId}/students/{studentId}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public StudentExamDTO GetStudentExam([FromRoute] int examId, [FromRoute] int studentId)
        {
            return examService.GetStudentExam(Util.GetJWTToken(Request.Headers), examId, studentId);
        }

        [Route("holding/{examId}/students/{studentId}/download")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult DownloadStudentExam([FromRoute] int examId, [FromRoute] int studentId)
        {
            return examService.GetStudentExamFile(Util.GetJWTToken(Request.Headers), examId, studentId);
        }

        [Route("holding/{examId}/students/{studentId}/tree/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<FileInfoDTO> GetStudentExamFileTree([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            return examService.GetStudentExamFileTree(Util.GetJWTToken(Request.Headers), examId, studentId, fileTreePath);
        }

        [Route("holding/{examId}/students/{studentId}/file/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public FileDTO GetStudentExamFileContent([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            return examService.GetStudentExamFileContent(Util.GetJWTToken(Request.Headers), examId, studentId, fileTreePath);
        }
    }
}