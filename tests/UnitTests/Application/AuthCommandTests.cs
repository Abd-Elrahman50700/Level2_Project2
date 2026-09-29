using Application.Common.Models;
using Application.Features.Auth.Commands;
using Application.Features.Auth.DTOs;
using Application.Features.Auth.Validators;
using Application.Interfaces;
using Moq;
using Xunit;

namespace UnitTests.Application;

public class AuthCommandTests
{
    private readonly Mock<IAuthService> _mockAuthService = new();

    [Fact]
    public async Task RegisterCommandHandler_ShouldDelegateToAuthService()
    {
        // Arrange
        var command = new RegisterCommand("newuser", "user@example.com", "Password123!", "New User");
        var expectedResponse = ApiResponse<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = "user-1",
            UserName = "newuser",
            Email = "user@example.com",
            AccessToken = "access.token.jwt",
            RefreshToken = "refresh-token"
        });

        _mockAuthService.Setup(s => s.RegisterAsync(It.Is<RegisterDto>(d =>
            d.UserName == command.UserName && d.Email == command.Email), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new RegisterCommandHandler(_mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("access.token.jwt", result.Data!.AccessToken);
        _mockAuthService.Verify(s => s.RegisterAsync(It.IsAny<RegisterDto>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginCommandHandler_ShouldDelegateToAuthService()
    {
        // Arrange
        var command = new LoginCommand("user@example.com", "Password123!");
        var expectedResponse = ApiResponse<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = "user-1",
            UserName = "existinguser",
            Email = "user@example.com",
            AccessToken = "access.token.jwt",
            RefreshToken = "refresh-token"
        });

        _mockAuthService.Setup(s => s.LoginAsync(It.Is<LoginDto>(d =>
            d.EmailOrUserName == command.EmailOrUserName && d.Password == command.Password), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new LoginCommandHandler(_mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("access.token.jwt", result.Data!.AccessToken);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_ShouldDelegateToAuthService()
    {
        // Arrange
        var command = new RefreshTokenCommand("old.access.token", "valid-refresh-token");
        var expectedResponse = ApiResponse<AuthResponseDto>.Success(new AuthResponseDto
        {
            UserId = "user-1",
            AccessToken = "new.access.token",
            RefreshToken = "new-refresh-token"
        });

        _mockAuthService.Setup(s => s.RefreshTokenAsync(It.Is<RefreshTokenDto>(d =>
            d.AccessToken == command.AccessToken && d.RefreshToken == command.RefreshToken), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new RefreshTokenCommandHandler(_mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("new.access.token", result.Data!.AccessToken);
    }

    [Fact]
    public async Task LogoutCommandHandler_ShouldDelegateToAuthService()
    {
        // Arrange
        var command = new LogoutCommand("user-1", "refresh-token-to-revoke");
        var expectedResponse = ApiResponse<bool>.Success(true, "Logged out successfully.");

        _mockAuthService.Setup(s => s.LogoutAsync(command.UserId, command.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResponse);

        var handler = new LogoutCommandHandler(_mockAuthService.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public void RegisterCommandValidator_ShouldPass_ForValidInput()
    {
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand("validuser", "valid@example.com", "Password123!", "Valid User");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void RegisterCommandValidator_ShouldFail_ForInvalidEmailOrShortPassword()
    {
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand("u", "notanemail", "123", null);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Email");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
        Assert.Contains(result.Errors, e => e.PropertyName == "UserName");
    }

    [Fact]
    public void LoginCommandValidator_ShouldFail_ForEmptyCredentials()
    {
        var validator = new LoginCommandValidator();
        var command = new LoginCommand("", "");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EmailOrUserName");
        Assert.Contains(result.Errors, e => e.PropertyName == "Password");
    }

    [Fact]
    public void RefreshTokenCommandValidator_ShouldFail_ForEmptyTokens()
    {
        var validator = new RefreshTokenCommandValidator();
        var command = new RefreshTokenCommand("", "");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "AccessToken");
        Assert.Contains(result.Errors, e => e.PropertyName == "RefreshToken");
    }
}
