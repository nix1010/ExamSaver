using ExamSaver.Configs;
using ExamSaver.Data;
using ExamSaver.Exceptions;
using ExamSaver.Models;
using ExamSaver.Models.API;
using ExamSaver.Models.Entity;
using ExamSaver.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ExamSaver.Services
{
    public class UserService
    {
        private readonly DatabaseContext databaseContext;
        private readonly AppSettings appSettings;

        public UserService(DatabaseContext databaseContext, IOptions<AppSettings> appSettings)
        {
            this.databaseContext = databaseContext;
            this.appSettings = appSettings.Value;
        }

        public JWTTokenDTO Authenticate(UserDTO userDTO)
        {
            string encryptedPassword = Util.Encrypt(userDTO.Password);

            User user = databaseContext
                .Users
                .Include(user => user.Roles)
                .Where(user => user.Email == userDTO.Email && user.Password == encryptedPassword)
                .FirstOrDefault();

            if (user == null)
            {
                throw new UserNotFoundException("Username or password is incorrect");
            }

            DateTime issuedDate = DateTime.Now;
            DateTime expiringDateTime = issuedDate.AddHours(8);

            return new JWTTokenDTO()
            {
                Token = GenerateJWTToken(user, expiringDateTime),
                IssuedAt = issuedDate,
                ExpiresAt = expiringDateTime,
                Roles = user.Roles.Select(role => role.Name).ToList()
            };
        }

        private string GenerateJWTToken(User user, DateTime expiryDateTime)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            byte[] secretKey = Encoding.ASCII.GetBytes(appSettings.JWTSecretKey);

            List<Claim> claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.NameId, user.Id.ToString())
            };

            foreach (Role roleEntity in user.Roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleEntity.Name));
            }

            SecurityTokenDescriptor securityTokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiryDateTime,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            SecurityToken securityToken = jwtSecurityTokenHandler.CreateToken(securityTokenDescriptor);

            return jwtSecurityTokenHandler.WriteToken(securityToken);
        }

        public int GetUserIdFromToken(string token)
        {
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwtSecurityToken = jwtSecurityTokenHandler.ReadJwtToken(token);

            foreach (Claim claim in jwtSecurityToken.Claims)
            {
                if (claim.Type.Equals(JwtRegisteredClaimNames.NameId))
                {
                    int userId = Convert.ToInt32(claim.Value);

                    if (databaseContext.Users.Find(userId) != null)
                    {
                        return userId;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            throw new UserNotFoundException("User id not found");
        }
    }
}
