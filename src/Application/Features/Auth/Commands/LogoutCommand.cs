using Application.Common.Models;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LogoutCommand(string? UserId, string RefreshToken) : IRequest<ApiResponse<bool>>;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponse<bool>>
{
    private readonly IAuthService _authService;

    public LogoutCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResponse<bool>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        return await _authService.LogoutAsync(request.UserId, request.RefreshToken, cancellationToken);
    }
}
