using Application.Common.Exceptions;
using Application.Features.Tasks.Commands;
using Application.Features.Tasks.Validators;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Moq;
using Xunit;

namespace UnitTests.Application;

public class UpdateTaskCommandTests
{
    private readonly Mock<ITaskRepository> _mockTaskRepository = new();
    private readonly Mock<IProjectRepository> _mockProjectRepository = new();
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();

    public UpdateTaskCommandTests()
    {
        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepository.Object);
        _mockUnitOfWork.Setup(u => u.Projects).Returns(_mockProjectRepository.Object);
    }

    [Fact]
    public async Task Handle_ShouldPreventInvalidTransition_WhenAttemptedViaUpdateTask()
    {
        // Arrange: Task in Todo cannot jump directly to Completed via general update
        var task = new TaskEntity("Old Title", "Old Desc", TaskPriority.Low, 1);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockProjectRepository.Setup(p => p.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var handler = new UpdateTaskCommandHandler(_mockUnitOfWork.Object);
        var command = new UpdateTaskCommand(1, "New Title", "New Desc", TaskPriority.High, TaskStatus.Completed, null, 1);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidTaskStatusTransitionException>(() => handler.Handle(command, CancellationToken.None));
        _mockTaskRepository.Verify(r => r.Update(It.IsAny<TaskEntity>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldAllowAllowedTransition_ViaUpdateTask()
    {
        // Arrange: Todo -> InProgress is allowed
        var task = new TaskEntity("Old Title", "Old Desc", TaskPriority.Low, 1);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockProjectRepository.Setup(p => p.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockTaskRepository.Setup(r => r.GetTaskWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new UpdateTaskCommandHandler(_mockUnitOfWork.Object);
        var command = new UpdateTaskCommand(1, "Updated Title", "Updated Desc", TaskPriority.High, TaskStatus.InProgress, null, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(TaskStatus.InProgress, task.Status);
        Assert.Equal("Updated Title", task.Title);
        _mockTaskRepository.Verify(r => r.Update(task), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldNotThrow_WhenStatusIsUnchanged()
    {
        // Arrange: Status is Todo and stays Todo while title changes
        var task = new TaskEntity("Old Title", "Old Desc", TaskPriority.Low, 1);
        _mockTaskRepository.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);
        _mockProjectRepository.Setup(p => p.ExistsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _mockTaskRepository.Setup(r => r.GetTaskWithDetailsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(task);

        var handler = new UpdateTaskCommandHandler(_mockUnitOfWork.Object);
        var command = new UpdateTaskCommand(1, "Renamed Title", "Old Desc", TaskPriority.Low, TaskStatus.Todo, null, 1);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(TaskStatus.Todo, task.Status);
        Assert.Equal("Renamed Title", task.Title);
    }
}
