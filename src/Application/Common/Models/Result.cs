using System.Text.Json.Serialization;

namespace Application.Common.Models;

public class Result
{
    public bool Succeeded { get; set; }
    public bool IsSuccess => Succeeded;
    public bool IsFailure => !Succeeded;
    public string Message { get; set; } = string.Empty;
    public IDictionary<string, string[]>? Errors { get; set; }
    public int? StatusCode { get; set; }

    public Result() { }

    protected Result(bool succeeded, string message, int? statusCode = null, IDictionary<string, string[]>? errors = null)
    {
        Succeeded = succeeded;
        Message = message;
        StatusCode = statusCode;
        Errors = errors;
    }

    public static Result Success(string message = "Operation completed successfully.")
        => new(true, message, 200);

    public static Result Failure(string message, int statusCode = 400, IDictionary<string, string[]>? errors = null)
        => new(false, message, statusCode, errors);

    public static Result Failure(string message, int statusCode, IEnumerable<string> errors)
        => new(false, message, statusCode, new Dictionary<string, string[]> { { "General", errors.ToArray() } });

    public static Result BadRequest(string message, IDictionary<string, string[]>? errors = null)
        => Failure(message, 400, errors);

    public static Result Unauthorized(string message = "Unauthorized access.")
        => Failure(message, 401);

    public static Result Forbidden(string message = "Access to this resource is forbidden.")
        => Failure(message, 403);

    public static Result NotFound(string message = "Resource not found.")
        => Failure(message, 404);

    public static Result Conflict(string message = "A conflict occurred with an existing resource.")
        => Failure(message, 409);

    public static Result InternalServerError(string message = "An unexpected error occurred.")
        => Failure(message, 500);
}

public class Result<T> : Result
{
    public T? Data { get; set; }

    [JsonIgnore]
    public T? Value => Data;

    public Result() { }

    public Result(bool succeeded, T? data, string message, int? statusCode = null, IDictionary<string, string[]>? errors = null)
        : base(succeeded, message, statusCode, errors)
    {
        Data = data;
    }

    public static Result<T> Success(T data, string message = "Success")
        => new(true, data, message, 200);

    public static new Result<T> Failure(string message, int statusCode = 400, IDictionary<string, string[]>? errors = null)
        => new(false, default, message, statusCode, errors);

    public static new Result<T> Failure(string message, int statusCode, IEnumerable<string> errors)
        => new(false, default, message, statusCode, new Dictionary<string, string[]> { { "General", errors.ToArray() } });

    public static new Result<T> BadRequest(string message, IDictionary<string, string[]>? errors = null)
        => Failure(message, 400, errors);

    public static new Result<T> Unauthorized(string message = "Unauthorized access.")
        => Failure(message, 401);

    public static new Result<T> Forbidden(string message = "Access to this resource is forbidden.")
        => Failure(message, 403);

    public static new Result<T> NotFound(string message = "Resource not found.")
        => Failure(message, 404);

    public static new Result<T> Conflict(string message = "A conflict occurred with an existing resource.")
        => Failure(message, 409);

    public static new Result<T> InternalServerError(string message = "An unexpected error occurred.")
        => Failure(message, 500);

    public static implicit operator Result<T>(T value) => Success(value);
}
