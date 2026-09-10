using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.Projects.Commands;

public record UpdateProjectCommand(int Id, string Name, string? Description) : IRequest<ApiResponse<ProjectDto>>;

public class UpdateProjectCommandHandler : IRequestHandler<UpdateProjectCommand, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(UpdateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.Id);
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
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TaskCount = project.Tasks?.Count ?? 0
        };

        return ApiResponse<ProjectDto>.Success(dto, "Project updated successfully.");
    }
}
