using Application.Common.Models;
using Application.Features.Auth.DTOs;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

[Authorize(Roles = Roles.Admin)]
[Authorize(Policy = Policies.RequireAdmin)]
public class AdminController : ApiControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly AppDbContext _context;

    public AdminController(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        AppDbContext context)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _context = context;
    }

    [HttpGet("users")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<UserDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAllUsers(CancellationToken cancellationToken)
    {
        var users = await _userManager.Users.ToListAsync(cancellationToken);
        var userDtos = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                Email = user.Email!,
                FullName = user.FullName,
                CreatedAt = user.CreatedAt,
                Roles = roles
            });
        }

        return Ok(ApiResponse<IReadOnlyList<UserDto>>.Success(userDtos));
    }

    [HttpPost("roles/assign")]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignRole([FromBody] AssignRoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return NotFound(ApiResponse<object>.Failure("User not found."));
        }

        if (!await _roleManager.RoleExistsAsync(request.Role))
        {
            return BadRequest(ApiResponse<object>.Failure($"Role '{request.Role}' does not exist."));
        }

        if (await _userManager.IsInRoleAsync(user, request.Role))
        {
            return BadRequest(ApiResponse<object>.Failure($"User already has role '{request.Role}'."));
        }

        var result = await _userManager.AddToRoleAsync(user, request.Role);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Failure("Failed to assign role.", result.Errors.Select(e => e.Description).ToArray()));
        }

        return Ok(ApiResponse<bool>.Success(true, $"Role '{request.Role}' assigned to user successfully."));
    }

    [HttpGet("system-summary")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemSummary(CancellationToken cancellationToken)
    {
        var totalUsers = await _userManager.Users.CountAsync(cancellationToken);
        var totalProjects = await _context.Projects.CountAsync(cancellationToken);
        var totalTasks = await _context.Tasks.CountAsync(cancellationToken);
        var totalComments = await _context.Comments.CountAsync(cancellationToken);

        var summary = new
        {
            TotalUsers = totalUsers,
            TotalProjects = totalProjects,
            TotalTasks = totalTasks,
            TotalComments = totalComments
        };

        return Ok(ApiResponse<object>.Success(summary, "System summary retrieved successfully."));
    }
}

public class AssignRoleRequest
{
    public string UserId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
