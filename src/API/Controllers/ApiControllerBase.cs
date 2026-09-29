using API.Common;
using Application.Common.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        var statusCode = result.StatusCode ?? StatusCodes.Status400BadRequest;
        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal Server Error"
        };

        var problemDetails = CustomProblemDetails.Create(
            statusCode,
            title,
            result.Message,
            HttpContext.Request.Path,
            result.Errors,
            HttpContext.TraceIdentifier);

        return StatusCode(statusCode, problemDetails);
    }

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return Ok(result);
        }

        var statusCode = result.StatusCode ?? StatusCodes.Status400BadRequest;
        var title = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad Request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not Found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "Internal Server Error"
        };

        var problemDetails = CustomProblemDetails.Create(
            statusCode,
            title,
            result.Message,
            HttpContext.Request.Path,
            result.Errors,
            HttpContext.TraceIdentifier);

        return StatusCode(statusCode, problemDetails);
    }
}
