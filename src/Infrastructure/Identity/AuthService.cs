using System.Security.Claims;
using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Identity;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly AppDbContext _context;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IJwtTokenGenerator jwtTokenGenerator,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _jwtTokenGenerator = jwtTokenGenerator;
        _context = context;
    }

    public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto request, CancellationToken cancellationToken = default)
    {
        var existingEmail = await _userManager.FindByEmailAsync(request.Email);
        if (existingEmail is not null)
        {
            return ApiResponse<AuthResponseDto>.Failure("A user with this email already exists.");
        }

        var existingUser = await _userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
        {
            return ApiResponse<AuthResponseDto>.Failure("A user with this username already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToArray();
            return ApiResponse<AuthResponseDto>.Failure("User registration failed.", errors);
        }

        // Ensure User role exists and assign it
        if (!await _roleManager.RoleExistsAsync(Roles.User))
        {
            await _roleManager.CreateAsync(new IdentityRole(Roles.User));
        }
        await _userManager.AddToRoleAsync(user, Roles.User);

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user, roleClaims);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt,
            Roles = roles
        };

        return ApiResponse<AuthResponseDto>.Success(response, "User registered successfully.");
    }

    public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.EmailOrUserName)
                   ?? await _userManager.FindByNameAsync(request.EmailOrUserName);

        if (user is null || !await _userManager.CheckPasswordAsync(user, request.Password))
        {
            return ApiResponse<AuthResponseDto>.Failure("Invalid email/username or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

        var (accessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user, roleClaims);
        var refreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = expiresAt,
            Roles = roles
        };

        return ApiResponse<AuthResponseDto>.Success(response, "Login successful.");
    }

    public async Task<ApiResponse<AuthResponseDto>> RefreshTokenAsync(RefreshTokenDto request, CancellationToken cancellationToken = default)
    {
        var principal = _jwtTokenGenerator.GetPrincipalFromExpiredToken(request.AccessToken);
        if (principal is null)
        {
            return ApiResponse<AuthResponseDto>.Failure("Invalid access token.");
        }

        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? principal.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return ApiResponse<AuthResponseDto>.Failure("Invalid token claims.");
        }

        var storedToken = await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == request.RefreshToken, cancellationToken);

        if (storedToken is null)
        {
            return ApiResponse<AuthResponseDto>.Failure("Refresh token does not exist.");
        }

        if (storedToken.UserId != userId)
        {
            return ApiResponse<AuthResponseDto>.Failure("Refresh token does not belong to the user.");
        }

        if (storedToken.IsRevoked)
        {
            return ApiResponse<AuthResponseDto>.Failure("Refresh token has been revoked.");
        }

        if (storedToken.IsExpired)
        {
            return ApiResponse<AuthResponseDto>.Failure("Refresh token has expired.");
        }

        var user = storedToken.User ?? await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApiResponse<AuthResponseDto>.Failure("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        var roleClaims = roles.Select(r => new Claim(ClaimTypes.Role, r)).ToList();

        // Token rotation: Revoke old refresh token and generate a new pair
        var (newAccessToken, expiresAt) = _jwtTokenGenerator.GenerateAccessToken(user, roleClaims);
        var newRefreshToken = _jwtTokenGenerator.GenerateRefreshToken(user.Id);

        storedToken.Revoke(newRefreshToken.Token);
        _context.RefreshTokens.Add(newRefreshToken);
        await _context.SaveChangesAsync(cancellationToken);

        var response = new AuthResponseDto
        {
            UserId = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken.Token,
            ExpiresAt = expiresAt,
            Roles = roles
        };

        return ApiResponse<AuthResponseDto>.Success(response, "Token refreshed successfully.");
    }

    public async Task<ApiResponse<bool>> LogoutAsync(string? userId, string refreshToken, CancellationToken cancellationToken = default)
    {
        var storedToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == refreshToken, cancellationToken);

        if (storedToken is not null && storedToken.IsActive)
        {
            if (!string.IsNullOrEmpty(userId) && storedToken.UserId != userId)
            {
                return ApiResponse<bool>.Failure("Refresh token does not belong to the user.");
            }

            storedToken.Revoke();
            await _context.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<bool>.Success(true, "Logged out successfully.");
    }

    public async Task<ApiResponse<UserDto>> GetCurrentUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return ApiResponse<UserDto>.Failure("User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        var dto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FullName = user.FullName,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };

        return ApiResponse<UserDto>.Success(dto);
    }
}
