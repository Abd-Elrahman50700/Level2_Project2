using MediatR;
using Application.Common.Models;
using Application.Features.Projects.DTOs;
using Application.Interfaces;

namespace Application.Features.Projects.Queries;

public record GetProjectsQuery : IRequest<ApiResponse<IReadOnlyList<ProjectDto>>>;

public class GetProjectsQueryHandler : IRequestHandler<GetProjectsQuery, ApiResponse<IReadOnlyList<ProjectDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetProjectsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IReadOnlyList<ProjectDto>>> Handle(GetProjectsQuery request, CancellationToken cancellationToken)
    {
        var projects = await _unitOfWork.Projects.GetProjectsWithTasksAsync(cancellationToken);
        var dtos = projects.Select(p => new ProjectDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            TaskCount = p.Tasks?.Count ?? 0
        }).ToList();

        return ApiResponse<IReadOnlyList<ProjectDto>>.Success(dtos);
    }
}
