using System.Collections.Generic;

namespace MealDelivery.Models.DTOs.Responses
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; } = true;
        public string Message { get; set; } = "Операция выполнена успешно";
        public T Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
    }

    public static class ApiResponse
    {
        public static ApiResponse<T> Success<T>(T data, string message = "Операция выполнена успешно")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Error<T>(string message = "Произошла ошибка", List<string> errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }

        public static ApiResponse<object> Success(string message = "Операция выполнена успешно")
        {
            return new ApiResponse<object>
            {
                IsSuccess = true,
                Message = message,
                Data = null
            };
        }

        public static ApiResponse<object> Error(string message = "Произошла ошибка", List<string> errors = null)
        {
            return new ApiResponse<object>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
    }
}