using Application.Common.Models;
using Application.Features.Auth.DTOs;

namespace Application.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto request, CancellationToken cancellationToken = default);
    Task<ApiResponse<bool>> LogoutAsync(string? userId, string refreshToken, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default);
}
