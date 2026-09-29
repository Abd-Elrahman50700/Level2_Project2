using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Projects.Commands;

public record DeleteProjectCommand(int Id) : IRequest<ApiResponse<bool>>;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.Id);
        }

        if (_currentUserService != null && !_currentUserService.IsAdmin)
        {
            var currentUserId = _currentUserService.UserId;
            if (project.UserId != null && project.UserId != currentUserId)
            {
                throw new ForbiddenException("You are not allowed to delete another user's project.");
            }
        }

        _unitOfWork.Projects.Delete(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, "Project deleted successfully.");
    }
}
