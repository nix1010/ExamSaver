using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExamSaver.Models.API
{
    public class AuthenticationResponseDTO
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public JWTTokenDTO JWTToken { get; set; }
    }
}
