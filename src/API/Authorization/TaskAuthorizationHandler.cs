using System.Security.Claims;
using Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using TaskEntity = Domain.Entities.Task;

namespace API.Authorization;

public class TaskAuthorizationHandler : AuthorizationHandler<ResourceOwnerOrAdminRequirement, TaskEntity>
{
    protected override System.Threading.Tasks.Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResourceOwnerOrAdminRequirement requirement,
        TaskEntity resource)
    {
        if (context.User.IsInRole(Roles.Admin))
        {
            context.Succeed(requirement);
            return System.Threading.Tasks.Task.CompletedTask;
        }

        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? context.User.FindFirst("sub")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            var isTaskOwner = resource.UserId == userId;
            var isProjectOwner = resource.Project?.UserId == userId;

            if (isTaskOwner || isProjectOwner)
            {
                context.Succeed(requirement);
            }
        }

        return System.Threading.Tasks.Task.CompletedTask;
    }
}
