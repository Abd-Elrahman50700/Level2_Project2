using MediatR;
using Application.Common.Models;
using Application.Features.Comments.Commands;
using Application.Features.Comments.DTOs;
using Application.Features.Comments.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class CommentsController : ApiControllerBase
{
    private readonly ISender _sender;

    public CommentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("task/{taskId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<CommentDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByTaskId(int taskId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCommentsByTaskIdQuery(taskId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<CommentDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateCommentDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateCommentCommand(dto.TaskId, dto.Content, dto.Author);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByTaskId), new { taskId = result.Data!.TaskId }, result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteCommentCommand(id), cancellationToken);
        return Ok(result);
    }
}
