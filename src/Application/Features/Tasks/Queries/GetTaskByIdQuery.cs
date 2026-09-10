using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Tasks.DTOs;
using Application.Interfaces;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Queries;

public record GetTaskByIdQuery(int Id) : IRequest<ApiResponse<TaskDto>>;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, ApiResponse<TaskDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetTaskByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<TaskDto>> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetTaskWithDetailsAsync(request.Id, cancellationToken);
        if (task is null)
        {
            throw new NotFoundException(nameof(TaskEntity), request.Id);
        }

        var dto = new TaskDto
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
        };

        return ApiResponse<TaskDto>.Success(dto);
    }
}
