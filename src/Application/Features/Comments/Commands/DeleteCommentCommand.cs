using Application.Common.Exceptions;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;
using MediatR;

namespace Application.Features.Comments.Commands;

public record DeleteCommentCommand(int Id) : IRequest<ApiResponse<bool>>;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService? _currentUserService;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService? currentUserService = null)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(request.Id, cancellationToken);
        if (comment is null)
        {
            throw new NotFoundException(nameof(Comment), request.Id);
        }

        if (_currentUserService != null && !_currentUserService.IsAdmin)
        {
            var currentUserId = _currentUserService.UserId;
            if (comment.UserId != null && comment.UserId != currentUserId)
            {
                throw new ForbiddenException("You are not allowed to delete another user's comment.");
            }
        }

        _unitOfWork.Comments.Delete(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, "Comment deleted successfully.");
    }
}
