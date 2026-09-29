using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces;
using MediatR;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Commands;

public record DeleteTaskCommand(int Id) : IRequest<ApiResponse<bool>>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
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
                throw new ForbiddenException("You are not allowed to delete another user's task.");
            }
        }

        _unitOfWork.Tasks.Delete(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, "Task deleted successfully.");
    }
}
