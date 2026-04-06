using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using MealDelivery.Models.DTOs.Responses;
using System;
using System.Threading.Tasks;

namespace MealDelivery.WebAPI.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла непредвиденная ошибка");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new ApiResponse<object>
            {
                IsSuccess = false,
                Message = "Внутренняя ошибка сервера",
                Errors = new List<string> { exception.Message }
            };

            if (exception is UnauthorizedAccessException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                response.Message = "Доступ запрещен";
            }
            else if (exception is KeyNotFoundException || exception is ArgumentException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                response.Message = "Ресурс не найден";
            }
            else if (exception is ValidationException)
            {
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "Ошибка валидации данных";
                response.Errors = exception.Message.Split(';').ToList();
            }
            else
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                if (!context.Request.Host.Value.Contains("localhost"))
                {
                    response.Errors = new List<string> { "Произошла внутренняя ошибка сервера" };
                }
            }

            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var jsonResponse = JsonSerializer.Serialize(response, options);
            return context.Response.WriteAsync(jsonResponse);
        }
    }

    public static class ExceptionHandlingMiddlewareExtensions
    {
        public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }

    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
        public ValidationException(string message, Exception inner) : base(message, inner) { }
    }
}