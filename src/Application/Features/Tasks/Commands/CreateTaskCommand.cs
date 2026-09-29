using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Tasks.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using MediatR;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Commands;

public record CreateTaskCommand(
    string Title,
    string? Description,
    TaskPriority Priority,
    TaskStatus Status,
    DateTime? DueDate,
    int ProjectId,
    string? UserId = null) : IRequest<ApiResponse<TaskDto>>;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public CreateTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<TaskDto>> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        // Users can only create tasks in their own project (Admin can create in any project)
        if (_currentUserService != null && !_currentUserService.IsAdmin)
        {
            var currentUserId = _currentUserService.UserId;
            if (project.UserId != null && project.UserId != currentUserId)
            {
                throw new ForbiddenException("You cannot create tasks in another user's project.");
            }
        }

        var currentUserIdValue = _currentUserService?.UserId;
        var isAdmin = _currentUserService?.IsAdmin ?? false;

        var assignedUserId = (isAdmin && !string.IsNullOrEmpty(request.UserId))
            ? request.UserId
            : (currentUserIdValue ?? request.UserId);

        var task = new TaskEntity(
            request.Title.Trim(),
            request.Description?.Trim(),
            request.Priority,
            request.ProjectId,
            request.DueDate,
            assignedUserId)
        {
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
            ProjectName = targetTask.Project?.Name ?? project.Name,
            UserId = targetTask.UserId,
            UserName = targetTask.User?.UserName ?? (targetTask.UserId == currentUserIdValue ? _currentUserService?.UserName : null),
            CreatedAt = targetTask.CreatedAt,
            UpdatedAt = targetTask.UpdatedAt,
            CommentCount = targetTask.Comments?.Count ?? 0
        };

        return ApiResponse<TaskDto>.Success(dto, "Task created successfully.");
    }
}
