using System.Net;
using System.Text.Json;
using API.Common;
using Application.Common.Exceptions;
using Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

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
        context.Response.ContentType = "application/problem+json";

        var problemDetails = exception switch
        {
            ValidationException validationEx => CustomProblemDetails.Create(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                validationEx.Message,
                context.Request.Path,
                validationEx.Errors,
                context.TraceIdentifier),

            DomainException domainEx => CustomProblemDetails.Create(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                domainEx.Message,
                context.Request.Path,
                traceId: context.TraceIdentifier),

            ArgumentException argEx => CustomProblemDetails.Create(
                StatusCodes.Status400BadRequest,
                "Bad Request",
                argEx.Message,
                context.Request.Path,
                traceId: context.TraceIdentifier),

            UnauthorizedAccessException unauthorizedEx => CustomProblemDetails.Create(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthorizedEx.Message,
                context.Request.Path,
                traceId: context.TraceIdentifier),

            UnauthorizedException unauthEx => CustomProblemDetails.Create(
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthEx.Message,
                context.Request.Path,
                traceId: context.TraceIdentifier),

            ForbiddenException forbiddenEx => CustomProblemDetails.Create(
                StatusCodes.Status403Forbidden,
                "Forbidden",
                forbiddenEx.Message,
                context.Request.Path,
                traceId: context.TraceIdentifier),

            NotFoundException notFoundEx => CustomProblemDetails.Create(
                StatusCodes.Status404NotFound,
                "Not Found",
                notFoundEx.Message,
                context.Request.Path,
                traceId: context.TraceIdentifier),

            ConflictException conflictEx => CustomProblemDetails.Create(
                StatusCodes.Status409Conflict,
                "Conflict",
                conflictEx.Message,
                context.Request.Path,
                traceId: context.TraceIdentifier),

            DbUpdateConcurrencyException => CustomProblemDetails.Create(
                StatusCodes.Status409Conflict,
                "Conflict",
                "A concurrency conflict occurred while updating the resource.",
                context.Request.Path,
                traceId: context.TraceIdentifier),

            _ => CustomProblemDetails.Create(
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "An unexpected error occurred. Please try again later.",
                context.Request.Path,
                traceId: context.TraceIdentifier)
        };

        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, jsonOptions));
    }
}
