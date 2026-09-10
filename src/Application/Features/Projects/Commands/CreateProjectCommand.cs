using MediatR;
using Application.Common.Models;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.Projects.Commands;

public record CreateProjectCommand(string Name, string? Description) : IRequest<ApiResponse<ProjectDto>>;

public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(CreateProjectCommand request, CancellationToken cancellationToken)
    {
        var project = new Project
        {
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Projects.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TaskCount = 0
        };

        return ApiResponse<ProjectDto>.Success(dto, "Project created successfully.");
    }
}
