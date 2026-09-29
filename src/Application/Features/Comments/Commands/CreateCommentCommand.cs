using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Features.Comments.DTOs;
using Application.Interfaces;
using Domain.Entities;
using MediatR;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Comments.Commands;

public record CreateCommentCommand(int TaskId, string Content, string? Author = null, string? UserId = null) : IRequest<ApiResponse<CommentDto>>;

public class CreateCommentCommandHandler : IRequestHandler<CreateCommentCommand, ApiResponse<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public CreateCommentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<CommentDto>> Handle(CreateCommentCommand request, CancellationToken cancellationToken)
    {
        var taskExists = await _unitOfWork.Tasks.ExistsAsync(request.TaskId, cancellationToken);
        if (!taskExists)
        {
            throw new NotFoundException(nameof(TaskEntity), request.TaskId);
        }

        var currentUserId = _currentUserService?.UserId;
        var currentUserName = _currentUserService?.UserName;

        var author = !string.IsNullOrWhiteSpace(request.Author)
            ? request.Author.Trim()
            : (!string.IsNullOrWhiteSpace(currentUserName) ? currentUserName : "User");

        var assignedUserId = request.UserId ?? currentUserId;

        var comment = new Comment
        {
            TaskId = request.TaskId,
            Content = request.Content.Trim(),
            Author = author,
            UserId = assignedUserId,
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
