using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Services.Interfaces;
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
        private readonly IExamService examService;
        private readonly IUserService userService;
        private readonly ISimilarityService similarityService;

        public ExamController(IExamService examService, IUserService userService, ISimilarityService similarityService)
        {
            this.examService = examService;
            this.userService = userService;
            this.similarityService = similarityService;
        }

        [Route("taking")]
        [HttpGet]
        [Authorize(Roles = RoleType.STUDENT)]
        public IList<ExamDTO> GetTakingExams([FromQuery] int? page)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            PagedList<ExamDTO> exams = examService.GetTakingExams(userId, Util.GetPage(page));

            Util.SetPageHeader(Response, exams.Page);

            return exams;
        }

        [Route("taking/{examId}")]
        [HttpGet]
        [Authorize(Roles = RoleType.STUDENT)]
        public ExamDTO GetTakingExam([FromRoute] int examId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return examService.GetTakingExam(userId, examId);
        }

        [Route("taking/{examId}")]
        [HttpPost]
        [Authorize(Roles = RoleType.STUDENT)]
        public IActionResult SubmitWork([FromRoute] int examId, [FromForm] IFormCollection form)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            examService.SubmitWork(userId, examId, form.Files);

            return Created(string.Empty, null);
        }

        [Route("holding")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<ExamDTO> GetHoldingExams([FromQuery] int? page)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            PagedList<ExamDTO> exams = examService.GetHoldingExams(userId, Util.GetPage(page));

            Util.SetPageHeader(Response, exams.Page);

            return exams;
        }

        [Route("holding/{examId}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public ExamDTO GetHoldingExam([FromRoute] int examId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return examService.GetHoldingExam(userId, examId);
        }

        [Route("holding")]
        [HttpPost]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult AddExam([FromBody] ExamDTO examDTO)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            examService.AddExam(userId, examDTO);

            return Created(string.Empty, null);
        }

        [Route("holding/{examId}")]
        [HttpPut]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult UpdateExam([FromRoute] int examId, [FromBody] ExamDTO examDTO)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            examService.UpdateExam(userId, examId, examDTO);

            return NoContent();
        }

        [Route("holding/{examId}")]
        [HttpDelete]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult DeleteExam([FromRoute] int examId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            examService.DeleteExam(userId, examId);

            return NoContent();
        }

        [Route("holding/{examId}/students")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<StudentExamDTO> GetStudentExams([FromRoute] int examId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return examService.GetStudentExams(userId, examId);
        }

        [Route("holding/{examId}/students/similarity")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<SimilarityResultDTO> GetSimilarityResults([FromRoute] int examId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return similarityService.GetSimilarityResults(userId, examId);
        }

        [Route("holding/{examId}/students/similarity")]
        [HttpPost]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult PerformSimilarityCheck([FromRoute] int examId, [FromBody] SimilarityRequestDTO similarityRequestDTO)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return Created(string.Empty, similarityService.PerformSimilarityCheck(userId, examId, similarityRequestDTO));
        }

        [Route("holding/{examId}/students/similarity/{similarityResultId}")]
        [HttpDelete]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult DeleteSimilarityResult([FromRoute] int examId, [FromRoute] int similarityResultId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            similarityService.DeleteSimilarityResult(userId, examId, similarityResultId);

            return NoContent();
        }

        [Route("holding/{examId}/students/{studentId}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public StudentExamDTO GetStudentExam([FromRoute] int examId, [FromRoute] int studentId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return examService.GetStudentExam(userId, examId, studentId);
        }

        [Route("holding/{examId}/students/{studentId}/download")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IActionResult DownloadStudentExam([FromRoute] int examId, [FromRoute] int studentId)
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return examService.GetStudentExamFile(userId, examId, studentId);
        }

        [Route("holding/{examId}/students/{studentId}/tree/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public IList<FileInfoDTO> GetStudentExamFileTree([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return examService.GetStudentExamFileTree(userId, examId, studentId, fileTreePath);
        }

        [Route("holding/{examId}/students/{studentId}/file/{**fileTreePath}")]
        [HttpGet]
        [Authorize(Roles = RoleType.PROFESSOR)]
        public FileDTO GetStudentExamFileContent([FromRoute] int examId, [FromRoute] int studentId, [FromRoute] string fileTreePath = "")
        {
            int userId = userService.GetUserIdFromToken(Util.GetJWTToken(Request.Headers));

            return examService.GetStudentExamFileContent(userId, examId, studentId, fileTreePath);
        }
    }
}