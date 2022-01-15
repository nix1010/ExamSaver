using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Services.Interfaces
{
    public interface IUserService
    {
        public AuthenticationResponseDTO Authenticate(UserDTO userDTO);

        public int GetUserIdFromToken(string token);
    }
}
