namespace Application.Common.Models;

public class ApiResponse<T> : Result<T>
{
    public ApiResponse() { }

    public ApiResponse(bool succeeded, T? data, string message, int? statusCode = null, IDictionary<string, string[]>? errors = null)
        : base(succeeded, data, message, statusCode, errors)
    {
    }

    public static new ApiResponse<T> Success(T data, string message = "Success")
    {
        return new ApiResponse<T>(true, data, message, 200);
    }

    public static ApiResponse<T> Failure(string message, IDictionary<string, string[]>? errors = null)
    {
        return new ApiResponse<T>(false, default, message, 400, errors);
    }

    public static ApiResponse<T> Failure(string message, IEnumerable<string> errors)
    {
        return new ApiResponse<T>(false, default, message, 400, new Dictionary<string, string[]>
        {
            { "General", errors.ToArray() }
        });
    }
}
