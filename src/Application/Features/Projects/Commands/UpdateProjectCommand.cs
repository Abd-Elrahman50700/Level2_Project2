using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Projects.Commands;

public record UpdateProjectCommand(int Id, string Name, string? Description) : IRequest<ApiResponse<ProjectDto>>;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
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
                throw new ForbiddenException("You are not allowed to modify another user's project.");
            }
        }

        project.Name = request.Name.Trim();
        project.Description = request.Description?.Trim();
        project.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Projects.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            UserId = project.UserId,
            UserName = project.User?.UserName,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TaskCount = project.Tasks?.Count ?? 0
        };

        return ApiResponse<ProjectDto>.Success(dto, "Project updated successfully.");
    }
}
