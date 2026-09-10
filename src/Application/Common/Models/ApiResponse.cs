namespace Application.Common.Models;

public class ApiResponse<T>
{
    public bool Succeeded { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IDictionary<string, string[]>? Errors { get; set; }

    public static ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Succeeded = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Failure(string message, IDictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>
        {
            Succeeded = false,
            Message = message,
            Errors = errors
        };
    }
}
