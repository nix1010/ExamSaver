using ExamSaver.Exceptions;
using ExamSaver.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ExamSaver.Utils
{
    public static class Util
    {
        public static string Encrypt(string text)
        {
            SHA256 sha256 = SHA256.Create();

            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));

            return Encoding.UTF8.GetString(hash);
        }

        public static string GetJWTToken(IHeaderDictionary headerDictionary)
        {
            headerDictionary.TryGetValue("Authorization", out StringValues token);

            return token.ToString().Replace("Bearer", "").Trim();
        }

        public static string GetStudentResourceIdentifier(Student student, int examId)
        {
            return $"{examId}-{student.Id}-{student.User.FirstName}-{student.User.LastName}-{student.Index}";
        }
    }
}
