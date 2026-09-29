using Application.Behaviors;
using Application.Features.Projects.Commands;
using Application.Features.Projects.Validators;
using FluentValidation;
using MediatR;
using Xunit;
using AppValidationException = Application.Common.Exceptions.ValidationException;

namespace UnitTests.Application;

public class ValidationBehaviorPipelineTests
{
    [Fact]
    public async Task ValidationBehavior_ShouldCallNext_WhenValidationPasses()
    {
        // Arrange
        var validators = new List<IValidator<CreateProjectCommand>>
        {
            new CreateProjectCommandValidator()
        };
        var behavior = new ValidationBehavior<CreateProjectCommand, int>(validators);
        var command = new CreateProjectCommand("Valid Project Name", "Description");

        var nextCalled = false;
        RequestHandlerDelegate<int> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(123);
        };

        // Act
        var result = await behavior.Handle(command, next, CancellationToken.None);

        // Assert
        Assert.True(nextCalled);
        Assert.Equal(123, result);
    }

    [Fact]
    public async Task ValidationBehavior_ShouldThrowValidationException_WhenValidationFails()
    {
        // Arrange
        var validators = new List<IValidator<CreateProjectCommand>>
        {
            new CreateProjectCommandValidator()
        };
        var behavior = new ValidationBehavior<CreateProjectCommand, int>(validators);
        var invalidCommand = new CreateProjectCommand("", "Description");

        var nextCalled = false;
        RequestHandlerDelegate<int> next = _ =>
        {
            nextCalled = true;
            return Task.FromResult(123);
        };

        // Act & Assert
        var ex = await Assert.ThrowsAsync<AppValidationException>(() => behavior.Handle(invalidCommand, next, CancellationToken.None));
        Assert.False(nextCalled);
        Assert.Contains("Name", ex.Errors.Keys);
        Assert.Contains(ex.Errors["Name"], msg => msg.Contains("required"));
    }

    [Fact]
    public async Task ValidationBehavior_ShouldProceed_WhenNoValidatorsRegistered()
    {
        // Arrange
        var validators = Enumerable.Empty<IValidator<CreateProjectCommand>>();
        var behavior = new ValidationBehavior<CreateProjectCommand, string>(validators);
        var command = new CreateProjectCommand("Any", null);

        var result = await behavior.Handle(command, _ => Task.FromResult("passed"), CancellationToken.None);

        // Assert
        Assert.Equal("passed", result);
    }
}
