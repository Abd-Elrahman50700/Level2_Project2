using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Comments.DTOs;
using Application.Interfaces;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Comments.Queries;

public record GetCommentsByTaskIdQuery(int TaskId) : IRequest<ApiResponse<IReadOnlyList<CommentDto>>>;

public class GetCommentsByTaskIdQueryHandler : IRequestHandler<GetCommentsByTaskIdQuery, ApiResponse<IReadOnlyList<CommentDto>>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCommentsByTaskIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<IReadOnlyList<CommentDto>>> Handle(GetCommentsByTaskIdQuery request, CancellationToken cancellationToken)
    {
        var taskExists = await _unitOfWork.Tasks.ExistsAsync(request.TaskId, cancellationToken);
        if (!taskExists)
        {
            throw new NotFoundException(nameof(TaskEntity), request.TaskId);
        }

        var comments = await _unitOfWork.Comments.GetCommentsByTaskIdAsync(request.TaskId, cancellationToken);
        var dtos = comments.Select(c => new CommentDto
        {
            Id = c.Id,
            Content = c.Content,
            Author = c.Author,
            TaskId = c.TaskId,
            CreatedAt = c.CreatedAt,
            UpdatedAt = c.UpdatedAt
        }).ToList();

        return ApiResponse<IReadOnlyList<CommentDto>>.Success(dtos);
    }
}
