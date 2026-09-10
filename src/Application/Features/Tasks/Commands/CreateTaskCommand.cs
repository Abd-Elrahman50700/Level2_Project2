using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Tasks.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Commands;

public record CreateTaskCommand(
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskItemStatus Status,
    DateTime? DueDate,
    int ProjectId) : IRequest<ApiResponse<TaskDto>>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var projectExists = await _unitOfWork.Projects.ExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        var task = new TaskEntity
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            Priority = request.Priority,
            Status = request.Status,
            DueDate = request.DueDate,
            ProjectId = request.ProjectId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Tasks.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(task.Id, cancellationToken);
        var targetTask = created ?? task;

        var dto = new TaskDto
        {
            Id = targetTask.Id,
            Title = targetTask.Title,
            Description = targetTask.Description,
            Priority = targetTask.Priority,
            Status = targetTask.Status,
            DueDate = targetTask.DueDate,
            ProjectId = targetTask.ProjectId,
            ProjectName = targetTask.Project?.Name,
            CreatedAt = targetTask.CreatedAt,
            UpdatedAt = targetTask.UpdatedAt,
            CommentCount = targetTask.Comments?.Count ?? 0
        };

        return ApiResponse<TaskDto>.Success(dto, "Task created successfully.");
    }
}
