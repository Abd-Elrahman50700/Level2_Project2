using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<ApiResponse<AuthResponseDto>>;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public RefreshTokenCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var dto = new RefreshTokenDto
        {
            AccessToken = request.AccessToken,
            RefreshToken = request.RefreshToken
        };

        return await _authService.RefreshTokenAsync(dto, cancellationToken);
    }
}
