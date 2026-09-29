using System.Text.Json;
using API.Common;
using API.Middleware;
using Application.Common.Exceptions;
using Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace UnitTests.Application;

public class ExceptionHandlingMiddlewareTests
{
    private static async Task<(int StatusCode, string ContentType, CustomProblemDetails Body)> ExecuteMiddlewareWithException(Exception exception)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test-resource";

        RequestDelegate next = _ => throw exception;
        var middleware = new ExceptionHandlingMiddleware(next, NullLogger<ExceptionHandlingMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var bodyText = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var problemDetails = JsonSerializer.Deserialize<CustomProblemDetails>(bodyText, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        })!;

        return (context.Response.StatusCode, context.Response.ContentType ?? string.Empty, problemDetails);
    }

    [Fact]
    public async Task Middleware_ShouldReturn400_WhenValidationExceptionThrown()
    {
        // Arrange
        var failures = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required." } }
        };
        var ex = new ValidationException(failures);

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(400, statusCode);
        Assert.Contains("application/problem+json", contentType);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.NotNull(problem.Errors);
        Assert.Contains("Name", problem.Errors.Keys);
        Assert.Equal("/api/test-resource", problem.Instance);
    }

    [Fact]
    public async Task Middleware_ShouldReturn400_WhenDomainExceptionThrown()
    {
        // Arrange
        var ex = new InvalidTaskStatusTransitionException(TaskStatus.Completed, TaskStatus.InProgress);

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(400, statusCode);
        Assert.Contains("application/problem+json", contentType);
        Assert.Equal("Bad Request", problem.Title);
        Assert.Equal(400, problem.Status);
        Assert.Contains("Cannot transition task status", problem.Detail);
    }

    [Fact]
    public async Task Middleware_ShouldReturn401_WhenUnauthorizedAccessExceptionThrown()
    {
        // Arrange
        var ex = new UnauthorizedAccessException("Invalid authentication credentials.");

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(401, statusCode);
        Assert.Contains("application/problem+json", contentType);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal(401, problem.Status);
        Assert.Equal("Invalid authentication credentials.", problem.Detail);
    }

    [Fact]
    public async Task Middleware_ShouldReturn401_WhenCustomUnauthorizedExceptionThrown()
    {
        // Arrange
        var ex = new UnauthorizedException("Session has expired.");

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(401, statusCode);
        Assert.Equal("Unauthorized", problem.Title);
        Assert.Equal(401, problem.Status);
        Assert.Equal("Session has expired.", problem.Detail);
    }

    [Fact]
    public async Task Middleware_ShouldReturn403_WhenForbiddenExceptionThrown()
    {
        // Arrange
        var ex = new ForbiddenException("You cannot modify another user's project.");

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(403, statusCode);
        Assert.Contains("application/problem+json", contentType);
        Assert.Equal("Forbidden", problem.Title);
        Assert.Equal(403, problem.Status);
        Assert.Equal("You cannot modify another user's project.", problem.Detail);
    }

    [Fact]
    public async Task Middleware_ShouldReturn404_WhenNotFoundExceptionThrown()
    {
        // Arrange
        var ex = new NotFoundException("Project", 42);

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(404, statusCode);
        Assert.Contains("application/problem+json", contentType);
        Assert.Equal("Not Found", problem.Title);
        Assert.Equal(404, problem.Status);
        Assert.Contains("42", problem.Detail);
    }

    [Fact]
    public async Task Middleware_ShouldReturn409_WhenConflictExceptionThrown()
    {
        // Arrange
        var ex = new ConflictException("A project with this name already exists.");

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(409, statusCode);
        Assert.Contains("application/problem+json", contentType);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal(409, problem.Status);
        Assert.Equal("A project with this name already exists.", problem.Detail);
    }

    [Fact]
    public async Task Middleware_ShouldReturn500_WhenUnhandledExceptionThrown()
    {
        // Arrange
        var ex = new InvalidOperationException("Fatal database crash.");

        // Act
        var (statusCode, contentType, problem) = await ExecuteMiddlewareWithException(ex);

        // Assert
        Assert.Equal(500, statusCode);
        Assert.Contains("application/problem+json", contentType);
        Assert.Equal("Internal Server Error", problem.Title);
        Assert.Equal(500, problem.Status);
        Assert.Contains("unexpected error occurred", problem.Detail);
    }
}
