using System.Security.Claims;
using Domain.Constants;
using Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace API.Authorization;

public class ProjectAuthorizationHandler : AuthorizationHandler<ResourceOwnerOrAdminRequirement, Project>
{
    protected override System.Threading.Tasks.Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerOrAdminRequirement requirement,
        Project resource)
    {
        if (context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId) && resource.UserId == userId)
        {
            context.Succeed(requirement);
        }

        return System.Threading.Tasks.Task.CompletedTask;
    }
}
