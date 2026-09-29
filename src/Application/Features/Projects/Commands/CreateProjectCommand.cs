using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Projects.Commands;

public record CreateProjectCommand(string Name, string? Description, string? UserId = null) : IRequest<ApiResponse<ProjectDto>>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService?.UserId;
        var isAdmin = _currentUserService?.IsAdmin ?? false;

        if (!isAdmin && !string.IsNullOrEmpty(request.UserId) && !string.IsNullOrEmpty(currentUserId) && request.UserId != currentUserId)
        {
            throw new ForbiddenException("Users cannot create projects for other users.");
        }

        var assignedUserId = (isAdmin && !string.IsNullOrEmpty(request.UserId))
            ? request.UserId
            : (currentUserId ?? request.UserId);

        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            UserId = assignedUserId,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            UserId = project.UserId,
            UserName = project.User?.UserName ?? (project.UserId == currentUserId ? _currentUserService?.UserName : null),
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TaskCount = 0
        };

        return ApiResponse<ProjectDto>.Success(dto, "Project created successfully.");
    }
}
