using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Tasks.DTOs;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.Tasks.Queries;

public record GetTasksByProjectIdQuery(int ProjectId) : IRequest<ApiResponse<IReadOnlyList<TaskDto>>>;

public class GetTasksByProjectIdQueryHandler : IRequestHandler<GetTasksByProjectIdQuery, ApiResponse<IReadOnlyList<TaskDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTasksByProjectIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IReadOnlyList<TaskDto>>> Handle(GetTasksByProjectIdQuery request, CancellationToken cancellationToken)
    {
        var projectExists = await _unitOfWork.Projects.ExistsAsync(request.ProjectId, cancellationToken);
        if (!projectExists)
        {
            throw new NotFoundException(nameof(Project), request.ProjectId);
        }

        var tasks = await _unitOfWork.Tasks.GetTasksByProjectIdAsync(request.ProjectId, cancellationToken);
        var dtos = tasks.Select(task => new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            DueDate = task.DueDate,
            ProjectId = task.ProjectId,
            ProjectName = task.Project?.Name,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt,
            CommentCount = task.Comments?.Count ?? 0
        }).ToList();

        return ApiResponse<IReadOnlyList<TaskDto>>.Success(dtos);
    }
}
