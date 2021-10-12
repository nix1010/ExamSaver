using ExamSaver.Models.API;
using ExamSaver.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ExamSaver.Controllers
{
    [ApiController]
    [Route("users")]
    public class UserController : ControllerBase
    {
        private readonly UserService userService;

        public UserController(UserService userService)
        {
            this.userService = userService;
        }

        [Route("authenticate")]
        [HttpPost]
        [AllowAnonymous]
        public JWTTokenDTO Authenticate([FromBody] UserDTO userDTO)
        {
            return userService.Authenticate(userDTO);
        }
    }
}
