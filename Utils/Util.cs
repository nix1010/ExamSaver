using ExamSaver.Configs;
using ExamSaver.Exceptions;
using ExamSaver.Models;
using ExamSaver.Models.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
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

        public static int GetPage(int? page)
        {
            int pageNumber = page.GetValueOrDefault(1);

            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            return pageNumber;
        }

        public static PagedList<T> ToPagedList<T>(this IQueryable<T> source, int pageNumber)
        {
            IList<T> paginatedResult = source
                .Skip((pageNumber - 1) * Constant.PAGE_SIZE)
                .Take(Constant.PAGE_SIZE)
                .ToList();

            Page page = new Page()
            {
                CurrentPage = pageNumber,
                PageSize = Constant.PAGE_SIZE,
                TotalCount = source.Count()
            };

            return new PagedList<T>(paginatedResult, page);
        }

        public static void SetPageHeader(HttpResponse httpResponse, Page page)
        {
            httpResponse.Headers.Add("X-Pagination", JsonSerializer.Serialize(page, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
        }
    }
}
