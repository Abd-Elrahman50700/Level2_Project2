using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public record RegisterCommand(string UserName, string Email, string Password, string? FullName) : IRequest<ApiResponse<AuthResponseDto>>;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public RegisterCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var dto = new RegisterDto
        {
            UserName = request.UserName,
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName
        };

        return await _authService.RegisterAsync(dto, cancellationToken);
    }
}
