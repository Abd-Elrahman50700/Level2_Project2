using MediatR;
using Application.Common.Models;
using Application.Features.Tasks.Commands;
using Application.Features.Tasks.DTOs;
using Application.Features.Tasks.Queries;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TasksController : ApiControllerBase
{
    private readonly ISender _sender;

    public TasksController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TaskDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTasksQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTaskByIdQuery(id), cancellationToken);
        return Ok(result);
    }

    [HttpGet("project/{projectId:int}")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<TaskDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByProjectId(int projectId, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTasksByProjectIdQuery(projectId), cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto, CancellationToken cancellationToken)
    {
        var command = new CreateTaskCommand(dto.Title, dto.Description, dto.Priority, dto.Status, dto.DueDate, dto.ProjectId);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<TaskDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto, CancellationToken cancellationToken)
    {
        var command = new UpdateTaskCommand(id, dto.Title, dto.Description, dto.Priority, dto.Status, dto.DueDate, dto.ProjectId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new DeleteTaskCommand(id), cancellationToken);
        return Ok(result);
    }
}
