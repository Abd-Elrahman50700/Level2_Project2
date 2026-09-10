using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.Comments.Commands;

public record DeleteCommentCommand(int Id) : IRequest<ApiResponse<bool>>;

public class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.Comments.GetByIdAsync(request.Id, cancellationToken);
        if (comment is null)
        {
            throw new NotFoundException(nameof(Comment), request.Id);
        }

        _unitOfWork.Comments.Delete(comment);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, "Comment deleted successfully.");
    }
}
