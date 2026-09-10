using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Interfaces;
using TaskEntity = Domain.Entities.Task;

namespace Application.Features.Tasks.Commands;

public record DeleteTaskCommand(int Id) : IRequest<ApiResponse<bool>>;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteTaskCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        var task = await _unitOfWork.Tasks.GetByIdAsync(request.Id, cancellationToken);
        if (task is null)
        {
            throw new NotFoundException(nameof(TaskEntity), request.Id);
        }

        _unitOfWork.Tasks.Delete(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, "Task deleted successfully.");
    }
}
