using System.Net;
using System.Text.Json;
using Application.Common.Exceptions;
using Application.Common.Models;

namespace API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            NotFoundException notFoundEx => new
            {
                StatusCode = (int)HttpStatusCode.NotFound,
                Body = ApiResponse<object>.Failure(notFoundEx.Message)
            },
            ValidationException validationEx => new
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Body = ApiResponse<object>.Failure(validationEx.Message, validationEx.Errors)
            },
            _ => new
            {
                StatusCode = (int)HttpStatusCode.InternalServerError,
                Body = ApiResponse<object>.Failure("An unexpected error occurred.")
            }
        };

        context.Response.StatusCode = response.StatusCode;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        await context.Response.WriteAsync(JsonSerializer.Serialize(response.Body, jsonOptions));
    }
}
