using Domain.Entities;
using Domain.Enums;
using Domain.Exceptions;
using Xunit;

namespace UnitTests.Domain;

public class TaskStatusWorkflowTests
{
    [Fact]
    public void NewTask_ShouldStartWith_TodoStatus()
    {
        // Act
        var task = new TaskEntity("Test Task", "Description", TaskPriority.Medium, 1);

        // Assert
        Assert.Equal(TaskStatus.Todo, task.Status);
    }

    [Theory]
    [InlineData(TaskStatus.Todo, TaskStatus.InProgress)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Completed)]
    [InlineData(TaskStatus.Todo, TaskStatus.Cancelled)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Cancelled)]
    public void CanTransitionTo_ShouldReturnTrue_ForAllowedTransitions(TaskStatus from, TaskStatus to)
    {
        // Arrange
        var task = CreateTaskInStatus(from);

        // Act
        var canTransition = task.CanTransitionTo(to);

        // Assert
        Assert.True(canTransition);
    }

    [Theory]
    [InlineData(TaskStatus.Todo, TaskStatus.InProgress)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Completed)]
    [InlineData(TaskStatus.Todo, TaskStatus.Cancelled)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Cancelled)]
    public void UpdateStatus_ShouldUpdateStatus_ForAllowedTransitions(TaskStatus from, TaskStatus to)
    {
        // Arrange
        var task = CreateTaskInStatus(from);

        // Act
        task.UpdateStatus(to);

        // Assert
        Assert.Equal(to, task.Status);
    }

    [Theory]
    // Forbidden: Terminal Completed state
    [InlineData(TaskStatus.Completed, TaskStatus.InProgress)]
    [InlineData(TaskStatus.Completed, TaskStatus.Todo)]
    [InlineData(TaskStatus.Completed, TaskStatus.Cancelled)]
    [InlineData(TaskStatus.Completed, TaskStatus.Completed)]
    // Forbidden: Terminal Cancelled state
    [InlineData(TaskStatus.Cancelled, TaskStatus.Completed)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.InProgress)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.Todo)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.Cancelled)]
    // Forbidden: Skipping workflow steps
    [InlineData(TaskStatus.Todo, TaskStatus.Completed)]
    // Forbidden: Reverting workflow steps
    [InlineData(TaskStatus.InProgress, TaskStatus.Todo)]
    // Forbidden: Same status transition
    [InlineData(TaskStatus.Todo, TaskStatus.Todo)]
    [InlineData(TaskStatus.InProgress, TaskStatus.InProgress)]
    public void CanTransitionTo_ShouldReturnFalse_ForInvalidTransitions(TaskStatus from, TaskStatus to)
    {
        // Arrange
        var task = CreateTaskInStatus(from);

        // Act
        var canTransition = task.CanTransitionTo(to);

        // Assert
        Assert.False(canTransition);
    }

    [Theory]
    // Example from user rules: Completed -> InProgress must be prevented
    [InlineData(TaskStatus.Completed, TaskStatus.InProgress)]
    // Example from user rules: Cancelled -> Completed must be prevented
    [InlineData(TaskStatus.Cancelled, TaskStatus.Completed)]
    // Other invalid transitions
    [InlineData(TaskStatus.Completed, TaskStatus.Cancelled)]
    [InlineData(TaskStatus.Cancelled, TaskStatus.InProgress)]
    [InlineData(TaskStatus.Todo, TaskStatus.Completed)]
    [InlineData(TaskStatus.InProgress, TaskStatus.Todo)]
    [InlineData(TaskStatus.Todo, TaskStatus.Todo)]
    public void UpdateStatus_ShouldThrowInvalidTaskStatusTransitionException_ForInvalidTransitions(TaskStatus from, TaskStatus to)
    {
        // Arrange
        var task = CreateTaskInStatus(from);

        // Act & Assert
        var exception = Assert.Throws<InvalidTaskStatusTransitionException>(() => task.UpdateStatus(to));
        Assert.Equal(from, exception.FromStatus);
        Assert.Equal(to, exception.ToStatus);
        Assert.Equal($"Cannot transition task status from '{from}' to '{to}'.", exception.Message);
    }

    [Fact]
    public void ChangeStatus_ShouldActAsAliasForUpdateStatus()
    {
        // Arrange
        var task = new TaskEntity("Test", "Desc", TaskPriority.Medium, 1);

        // Act
        task.ChangeStatus(TaskStatus.InProgress);

        // Assert
        Assert.Equal(TaskStatus.InProgress, task.Status);
    }

    private static TaskEntity CreateTaskInStatus(TaskStatus status)
    {
        var task = new TaskEntity("Test Task", "Description", TaskPriority.Medium, 1);
        if (status == TaskStatus.Todo)
        {
            return task;
        }

        if (status == TaskStatus.InProgress)
        {
            task.UpdateStatus(TaskStatus.InProgress);
            return task;
        }

        if (status == TaskStatus.Completed)
        {
            task.UpdateStatus(TaskStatus.InProgress);
            task.UpdateStatus(TaskStatus.Completed);
            return task;
        }

        if (status == TaskStatus.Cancelled)
        {
            task.UpdateStatus(TaskStatus.Cancelled);
            return task;
        }

        throw new ArgumentOutOfRangeException(nameof(status), status, null);
    }
}
