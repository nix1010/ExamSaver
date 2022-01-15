using ExamSaver.Models.API;
using ExamSaver.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamSaver.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService userService;

        public UserController(IUserService userService)
        {
            this.userService = userService;
        }

        [Route("authenticate")]
        [HttpPost]
        [AllowAnonymous]
        public AuthenticationResponseDTO Authenticate([FromBody] UserDTO userDTO)
        {
            return userService.Authenticate(userDTO);
        }
    }
}
