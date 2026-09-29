using System.Security.Claims;
using Domain.Entities;

namespace Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(ApplicationUser user, IEnumerable<Claim>? extraClaims = null);
    RefreshToken GenerateRefreshToken(string userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}
