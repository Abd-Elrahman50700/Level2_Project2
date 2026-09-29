using Domain.Entities;
using Domain.Enums;
using Xunit;

namespace UnitTests.Domain;

public class ApplicationUserTests
{
    [Fact]
    public void ApplicationUser_ShouldInitializeCollections()
    {
        // Arrange & Act
        var user = new ApplicationUser
        {
            UserName = "testuser",
            Email = "test@example.com",
            FullName = "Test User"
        };

        // Assert
        Assert.NotNull(user.Projects);
        Assert.NotNull(user.Tasks);
        Assert.NotNull(user.RefreshTokens);
        Assert.Empty(user.Projects);
        Assert.Empty(user.Tasks);
        Assert.Empty(user.RefreshTokens);
    }

    [Fact]
    public void Project_CanBeAssociatedWithUser()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-123", UserName = "creator" };
        var project = new Project
        {
            Id = 1,
            Name = "Clean Architecture App",
            UserId = user.Id,
            User = user
        };

        user.Projects.Add(project);

        // Assert
        Assert.Equal("user-123", project.UserId);
        Assert.Same(user, project.User);
        Assert.Single(user.Projects);
        Assert.Contains(project, user.Projects);
    }

    [Fact]
    public void Task_CanBeAssociatedWithUser()
    {
        // Arrange
        var user = new ApplicationUser { Id = "user-456", UserName = "developer" };
        var task = new TaskEntity("Implement Auth", "Add JWT", TaskPriority.High, 1, null, user.Id)
        {
            User = user
        };

        user.Tasks.Add(task);

        // Assert
        Assert.Equal("user-456", task.UserId);
        Assert.Same(user, task.User);
        Assert.Single(user.Tasks);
        Assert.Contains(task, user.Tasks);
    }

    [Fact]
    public void RefreshToken_IsActive_WhenNotExpiredAndNotRevoked()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "secure-random-token",
            UserId = "user-1",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        // Assert
        Assert.True(token.IsActive);
        Assert.False(token.IsExpired);
    }

    [Fact]
    public void RefreshToken_IsNotActive_WhenExpired()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "expired-token",
            UserId = "user-1",
            ExpiryDate = DateTime.UtcNow.AddMinutes(-5),
            IsRevoked = false
        };

        // Assert
        Assert.False(token.IsActive);
        Assert.True(token.IsExpired);
    }

    [Fact]
    public void RefreshToken_Revoke_ShouldSetRevokedFlagAndTimestamp()
    {
        // Arrange
        var token = new RefreshToken
        {
            Token = "token-to-revoke",
            UserId = "user-1",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsRevoked = false
        };

        // Act
        token.Revoke("replacement-token");

        // Assert
        Assert.True(token.IsRevoked);
        Assert.NotNull(token.RevokedAt);
        Assert.Equal("replacement-token", token.ReplacedByToken);
        Assert.False(token.IsActive);
    }
}
