using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Tasks.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Commands;

public record UpdateTaskCommand(
    int Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskItemStatus Status,
    DateTime? DueDate,
    int ProjectId) : IRequest<ApiResponse<TaskDto>>;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.Id, cancellationToken);
        if (task is null)
        {
            throw new NotFoundException(nameof(TaskEntity), request.Id);
        }

        var projectExists = await _unitOfWork.Projects.ExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.Status = request.Status;
        task.DueDate = request.DueDate;
        task.ProjectId = request.ProjectId;
        task.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Tasks.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(task.Id, cancellationToken);
        var targetTask = updated ?? task;

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

        return ApiResponse<TaskDto>.Success(dto, "Task updated successfully.");
    }
}
