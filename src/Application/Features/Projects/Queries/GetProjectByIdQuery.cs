using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Projects.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.Projects.Queries;

public record GetProjectByIdQuery(int Id) : IRequest<ApiResponse<ProjectDto>>;

public class GetProjectByIdQueryHandler : IRequestHandler<GetProjectByIdQuery, ApiResponse<ProjectDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<ProjectDto>> Handle(GetProjectByIdQuery request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetProjectWithTasksAsync(request.Id, cancellationToken);
        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.Id);
        }

        var dto = new ProjectDto
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt,
            TaskCount = project.Tasks?.Count ?? 0
        };

        return ApiResponse<ProjectDto>.Success(dto);
    }
}
