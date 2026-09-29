using Application.Common.Exceptions;
using Application.Features.Tasks.Commands;
using Application.Features.Tasks.Validators;
using Application.Interfaces;
using Domain.Enums;
using Domain.Exceptions;
using Moq;
using Xunit;

namespace UnitTests.Application;

public class UpdateTaskStatusCommandTests
{
    private readonly Mock<ITaskRepository> _mockTaskRepository = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();

    public UpdateTaskStatusCommandTests()
    {
        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldUpdateStatus_WhenTransitionIsValid()
    {
        // Arrange
        var task = new TaskEntity("Build Feature", "Workflow implementation", TaskPriority.High, 1);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockTaskRepository.Setup(r => r.GetTaskWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new UpdateTaskStatusCommandHandler(_mockUnitOfWork.Object);
        var command = new UpdateTaskStatusCommand(1, TaskStatus.InProgress);

        // Act
        var response = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(response.Succeeded);
        Assert.Equal(TaskStatus.InProgress, response.Data!.Status);
        Assert.Equal(TaskStatus.InProgress, task.Status);
        _mockTaskRepository.Verify(r => r.Update(task), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenTaskDoesNotExist()
    {
        // Arrange
        _mockTaskRepository.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TaskEntity?)null);

        var handler = new UpdateTaskStatusCommandHandler(_mockUnitOfWork.Object);
        var command = new UpdateTaskStatusCommand(999, TaskStatus.InProgress);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowInvalidTaskStatusTransitionException_WhenTransitionIsInvalid()
    {
        // Arrange: Task in Todo cannot jump directly to Completed
        var task = new TaskEntity("Build Feature", "Workflow implementation", TaskPriority.High, 1);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new UpdateTaskStatusCommandHandler(_mockUnitOfWork.Object);
        var command = new UpdateTaskStatusCommand(1, TaskStatus.Completed);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidTaskStatusTransitionException>(() => handler.Handle(command, CancellationToken.None));
        _mockTaskRepository.Verify(r => r.Update(It.IsAny<TaskEntity>()), Times.Never);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void Validator_ShouldPass_ForValidInput()
    {
        // Arrange
        var validator = new UpdateTaskStatusCommandValidator();
        var command = new UpdateTaskStatusCommand(1, TaskStatus.InProgress);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validator_ShouldFail_WhenIdIsInvalid()
    {
        // Arrange
        var validator = new UpdateTaskStatusCommandValidator();
        var command = new UpdateTaskStatusCommand(0, TaskStatus.InProgress);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void Validator_ShouldFail_WhenStatusIsOutOfRange()
    {
        // Arrange
        var validator = new UpdateTaskStatusCommandValidator();
        var command = new UpdateTaskStatusCommand(1, (TaskStatus)999);

        // Act
        var result = validator.Validate(command);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status");
    }
}
