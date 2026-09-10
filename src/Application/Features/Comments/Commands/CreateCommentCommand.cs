using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Features.Comments.DTOs;
using Application.Interfaces;
using Domain.Entities;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Comments.Commands;

public record CreateCommentCommand(int TaskId, string Content, string Author) : IRequest<ApiResponse<CommentDto>>;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ApiResponse<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var taskExists = await _unitOfWork.Tasks.ExistsAsync(request.TaskId, cancellationToken);
        if (!taskExists)
        {
            throw new NotFoundException(nameof(TaskEntity), request.TaskId);
        }

        var comment = new Comment
        {
            TaskId = request.TaskId,
            Content = request.Content.Trim(),
            Author = request.Author.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.Comments.AddAsync(comment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var dto = new CommentDto
        {
            Id = comment.Id,
            Content = comment.Content,
            Author = comment.Author,
            TaskId = comment.TaskId,
            CreatedAt = comment.CreatedAt,
            UpdatedAt = comment.UpdatedAt
        };

        return ApiResponse<CommentDto>.Success(dto, "Comment added successfully.");
    }
}
