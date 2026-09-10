using Application.Common.Exceptions;
using MediatR;
using Application.Common.Models;
using Application.Interfaces;
using Domain.Entities;

namespace Application.Features.Projects.Commands;

public record DeleteProjectCommand(int Id) : IRequest<ApiResponse<bool>>;

public class DeleteProjectCommandHandler : IRequestHandler<DeleteProjectCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteProjectCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteProjectCommand request, CancellationToken cancellationToken)
    {
        var project = await _unitOfWork.Projects.GetByIdAsync(request.Id, cancellationToken);
        if (project is null)
        {
            throw new NotFoundException(nameof(Project), request.Id);
        }

        _unitOfWork.Projects.Delete(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.Success(true, "Project deleted successfully.");
    }
}
