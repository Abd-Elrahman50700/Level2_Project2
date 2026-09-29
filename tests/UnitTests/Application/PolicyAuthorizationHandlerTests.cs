using System.Security.Claims;
using API.Authorization;
using Domain.Constants;
using Domain.Entities;
using global::Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace UnitTests.Application;

public class PolicyAuthorizationHandlerTests
{
    private readonly ResourceOwnerOrAdminRequirement _requirement = new();

    private static ClaimsPrincipal CreateUserPrincipal(string userId, string? role = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, $"user_{userId}")
        };

        if (!string.IsNullOrEmpty(role))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        return new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
    }

    [Fact]
    public async System.Threading.Tasks.Task ProjectHandler_ShouldSucceed_WhenUserIsAdmin()
    {
        // Arrange
        var handler = new ProjectAuthorizationHandler();
        var adminUser = CreateUserPrincipal("admin-id", Roles.Admin);
        var project = new Project { Id = 1, UserId = "other-user" };
        var context = new AuthorizationHandlerContext([_requirement], adminUser, project);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task ProjectHandler_ShouldSucceed_WhenUserIsOwner()
    {
        // Arrange
        var handler = new ProjectAuthorizationHandler();
        var ownerUser = CreateUserPrincipal("owner-id", Roles.User);
        var project = new Project { Id = 1, UserId = "owner-id" };
        var context = new AuthorizationHandlerContext([_requirement], ownerUser, project);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task ProjectHandler_ShouldFail_WhenUserIsNotOwnerNorAdmin()
    {
        // Arrange
        var handler = new ProjectAuthorizationHandler();
        var nonOwnerUser = CreateUserPrincipal("intruder-id", Roles.User);
        var project = new Project { Id = 1, UserId = "owner-id" };
        var context = new AuthorizationHandlerContext([_requirement], nonOwnerUser, project);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskHandler_ShouldSucceed_WhenUserIsAdmin()
    {
        // Arrange
        var handler = new TaskAuthorizationHandler();
        var adminUser = CreateUserPrincipal("admin-id", Roles.Admin);
        var task = new TaskEntity("Title", "Desc", TaskPriority.Low, 1, null, "user-1");
        var context = new AuthorizationHandlerContext([_requirement], adminUser, task);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskHandler_ShouldSucceed_WhenUserIsTaskOwner()
    {
        // Arrange
        var handler = new TaskAuthorizationHandler();
        var user = CreateUserPrincipal("user-1", Roles.User);
        var task = new TaskEntity("Title", "Desc", TaskPriority.Low, 1, null, "user-1");
        var context = new AuthorizationHandlerContext([_requirement], user, task);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskHandler_ShouldSucceed_WhenUserIsProjectOwner()
    {
        // Arrange
        var handler = new TaskAuthorizationHandler();
        var projectOwner = CreateUserPrincipal("project-owner", Roles.User);
        var task = new TaskEntity("Title", "Desc", TaskPriority.Low, 1, null, "assignee-user")
        {
            Project = new Project { Id = 1, UserId = "project-owner" }
        };
        var context = new AuthorizationHandlerContext([_requirement], projectOwner, task);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task TaskHandler_ShouldFail_WhenUserIsNotOwnerNorAdmin()
    {
        // Arrange
        var handler = new TaskAuthorizationHandler();
        var intruder = CreateUserPrincipal("intruder", Roles.User);
        var task = new TaskEntity("Title", "Desc", TaskPriority.Low, 1, null, "user-1")
        {
            Project = new Project { Id = 1, UserId = "user-2" }
        };
        var context = new AuthorizationHandlerContext([_requirement], intruder, task);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task CommentHandler_ShouldSucceed_WhenUserIsAdmin()
    {
        // Arrange
        var handler = new CommentAuthorizationHandler();
        var adminUser = CreateUserPrincipal("admin-id", Roles.Admin);
        var comment = new Comment { Id = 1, TaskId = 1, Content = "Text", UserId = "other-user" };
        var context = new AuthorizationHandlerContext([_requirement], adminUser, comment);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task CommentHandler_ShouldSucceed_WhenUserIsCommentOwner()
    {
        // Arrange
        var handler = new CommentAuthorizationHandler();
        var commentOwner = CreateUserPrincipal("comment-owner", Roles.User);
        var comment = new Comment { Id = 1, TaskId = 1, Content = "Text", UserId = "comment-owner" };
        var context = new AuthorizationHandlerContext([_requirement], commentOwner, comment);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public async System.Threading.Tasks.Task CommentHandler_ShouldFail_WhenUserIsNotOwnerNorAdmin()
    {
        // Arrange
        var handler = new CommentAuthorizationHandler();
        var intruder = CreateUserPrincipal("intruder", Roles.User);
        var comment = new Comment { Id = 1, TaskId = 1, Content = "Text", UserId = "comment-owner" };
        var context = new AuthorizationHandlerContext([_requirement], intruder, comment);

        // Act
        await handler.HandleAsync(context);

        // Assert
        Assert.False(context.HasSucceeded);
    }
}
