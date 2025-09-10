namespace Fanda.UserManagement.Api.Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = default!;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new();

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}

public static class ApiResponse
{
    public static ApiResponse<object> Success(string message = "Operation successful")
    {
        return new ApiResponse<object>
        {
            Success = true,
            Message = message,
            Data = null
        };
    }

    public static ApiResponse<object> Error(string message, List<string>? errors = null)
    {
        return new ApiResponse<object>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }
}
