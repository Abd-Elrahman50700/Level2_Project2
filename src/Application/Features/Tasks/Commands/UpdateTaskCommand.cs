using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Tasks.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Commands;

public record UpdateTaskCommand(
    int Id,
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime? DueDate,
    int ProjectId) : IRequest<ApiResponse<TaskDto>>;

public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public UpdateTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<TaskDto>> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(request.Id, cancellationToken)
                   ?? await _unitOfWork.Tasks.GetByIdAsync(request.Id, cancellationToken);

        if (task is null)
        {
            throw new NotFoundException(nameof(TaskEntity), request.Id);
        }

        // Check ownership vs Admin: Users should not be able to modify another user's resources
        if (_currentUserService != null && !_currentUserService.IsAdmin)
        {
            var currentUserId = _currentUserService.UserId;
            var isTaskOwner = task.UserId != null && task.UserId == currentUserId;
            var isProjectOwner = task.Project?.UserId != null && task.Project.UserId == currentUserId;

            if (!isTaskOwner && !isProjectOwner && (task.UserId != null || task.Project?.UserId != null))
            {
                throw new ForbiddenException("You are not allowed to modify another user's task.");
            }
        }

        var projectExists = await _unitOfWork.Projects.ExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        // If moving to another project, verify user owns target project (unless Admin)
        if (_currentUserService != null && !_currentUserService.IsAdmin && request.ProjectId != task.ProjectId)
        {
            var targetProject = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId, cancellationToken);
            var currentUserId = _currentUserService.UserId;
            if (targetProject?.UserId != null && targetProject.UserId != currentUserId)
            {
                throw new ForbiddenException("You cannot move tasks to another user's project.");
            }
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Priority = request.Priority;
        task.DueDate = request.DueDate;
        task.ProjectId = request.ProjectId;
        task.UpdatedAt = DateTime.UtcNow;

        if (task.Status != request.Status)
        {
            task.UpdateStatus(request.Status);
        }

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
            UserId = targetTask.UserId,
            UserName = targetTask.User?.UserName,
            CreatedAt = targetTask.CreatedAt,
            UpdatedAt = targetTask.UpdatedAt,
            CommentCount = targetTask.Comments?.Count ?? 0
        };

        return ApiResponse<TaskDto>.Success(dto, "Task updated successfully.");
    }
}
