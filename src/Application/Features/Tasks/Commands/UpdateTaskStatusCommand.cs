using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Tasks.DTOs;
using Application.Interfaces;
using Domain.Enums;
using MediatR;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Commands;

public record UpdateTaskStatusCommand(int Id, TaskStatus Status) : IRequest<ApiResponse<TaskDto>>;

public class UpdateTaskStatusCommandHandler : IRequestHandler<UpdateTaskStatusCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public UpdateTaskStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<TaskDto>> Handle(UpdateTaskStatusCommand request, CancellationToken cancellationToken)
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
                throw new ForbiddenException("You are not allowed to modify another user's task status.");
            }
        }

        task.UpdateStatus(request.Status);
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
            UserId = targetTask.UserId,
            UserName = targetTask.User?.UserName,
            CreatedAt = targetTask.CreatedAt,
            UpdatedAt = targetTask.UpdatedAt,
            CommentCount = targetTask.Comments?.Count ?? 0
        };

        return ApiResponse<TaskDto>.Success(dto, $"Task status updated to '{targetTask.Status}' successfully.");
    }
}
