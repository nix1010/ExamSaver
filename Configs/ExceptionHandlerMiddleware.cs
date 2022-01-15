using ExamSaver.Exceptions;
using ExamSaver.Models.API;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExamSaver.Configs
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate next;

        public ExceptionHandlerMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context, IWebHostEnvironment env)
        {
            try
            {
                await next(context);
            }
            catch (Exception error)
            {
                HttpResponse response = context.Response;

                HttpStatusCode httpStatusCode = error switch
                {
                    UserNotFoundException _ => HttpStatusCode.Unauthorized,
                    UnauthenticatedException _ => HttpStatusCode.Unauthorized,
                    BadRequestException _ => HttpStatusCode.BadRequest,
                    NotFoundException _ => HttpStatusCode.NotFound,
                    _ => HttpStatusCode.InternalServerError,
                };

                string message;

                if(!env.IsDevelopment() && httpStatusCode == HttpStatusCode.InternalServerError)
                {
                    message = "Unknown error occured";
                }
                else
                {
                    message = error.Message;
                }

                response.StatusCode = (int)httpStatusCode;
                
                response.ContentType = "application/json";

                string body = JsonSerializer.Serialize(new ErrorResponseDTO()
                {
                    Title = httpStatusCode.ToString(),
                    StatusCode = httpStatusCode,
                    Message = message
                }, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                await response.WriteAsync(body);
            }
        }
    }
}
