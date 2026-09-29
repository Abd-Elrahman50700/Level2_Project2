using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Commands;

public record LoginCommand(string EmailOrUserName, string Password) : IRequest<ApiResponse<AuthResponseDto>>;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<AuthResponseDto>>
{
    private readonly IAuthService _authService;

    public LoginCommandHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResponse<AuthResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var dto = new LoginDto
        {
            EmailOrUserName = request.EmailOrUserName,
            Password = request.Password
        };

        return await _authService.LoginAsync(dto, cancellationToken);
    }
}
