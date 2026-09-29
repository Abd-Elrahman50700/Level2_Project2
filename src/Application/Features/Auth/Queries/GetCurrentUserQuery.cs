using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Interfaces;
using MediatR;

namespace Application.Features.Auth.Queries;

public record GetCurrentUserQuery(string UserId) : IRequest<ApiResponse<UserDto>>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, ApiResponse<UserDto>>
{
    private readonly IAuthService _authService;

    public GetCurrentUserQueryHandler(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        return await _authService.GetCurrentUserAsync(request.UserId, cancellationToken);
    }
}
