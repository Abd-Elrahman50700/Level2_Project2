using Application.Common.Exceptions;
using Application.Features.Comments.Commands;
using Application.Features.Projects.Commands;
using Application.Features.Tasks.Commands;
using Application.Interfaces;
using Domain.Constants;
using Domain.Entities;
using Domain.Enums;
using Moq;
using Xunit;

namespace UnitTests.Application;

public class ResourceAuthorizationTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork = new();
    private readonly Mock<IProjectRepository> _mockProjectRepo = new();
    private readonly Mock<ITaskRepository> _mockTaskRepo = new();
    private readonly Mock<ICommentRepository> _mockCommentRepo = new();
    private readonly Mock<ICurrentUserService> _mockCurrentUserService = new();

    public ResourceAuthorizationTests()
    {
        _mockUnitOfWork.Setup(u => u.Projects).Returns(_mockProjectRepo.Object);
        _mockUnitOfWork.Setup(u => u.Tasks).Returns(_mockTaskRepo.Object);
        _mockUnitOfWork.Setup(u => u.Comments).Returns(_mockCommentRepo.Object);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_ShouldModifyOwnProject_Successfully()
    {
        // Arrange
        const string userId = "user-123";
        _mockCurrentUserService.Setup(u => u.UserId).Returns(userId);
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var project = new Project { Id = 1, Name = "My Project", UserId = userId };
        _mockProjectRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var handler = new UpdateProjectCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new UpdateProjectCommand(1, "Updated Project Name", "Updated Desc");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Updated Project Name", project.Name);
        _mockProjectRepo.Verify(r => r.Update(project), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CannotModifyAnotherUsersProject_ThrowsForbiddenException()
    {
        // Arrange
        const string currentUserId = "user-123";
        const string otherUserId = "user-456";
        _mockCurrentUserService.Setup(u => u.UserId).Returns(currentUserId);
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var otherUserProject = new Project { Id = 1, Name = "Other User's Project", UserId = otherUserId };
        _mockProjectRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(otherUserProject);

        var handler = new UpdateProjectCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new UpdateProjectCommand(1, "Hacked Project", "Hacked Desc");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("not allowed to modify", ex.Message);
        _mockProjectRepo.Verify(r => r.Update(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task Admin_CanModifyAnyProject_Successfully()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("admin-user");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(true);

        var otherUserProject = new Project { Id = 1, Name = "User's Project", UserId = "user-456" };
        _mockProjectRepo.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(otherUserProject);

        var handler = new UpdateProjectCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new UpdateProjectCommand(1, "Admin Modified Project", "Admin Description");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("Admin Modified Project", otherUserProject.Name);
        _mockProjectRepo.Verify(r => r.Update(otherUserProject), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CannotDeleteAnotherUsersProject_ThrowsForbiddenException()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("user-1");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var project = new Project { Id = 5, Name = "User 2 Project", UserId = "user-2" };
        _mockProjectRepo.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        var handler = new DeleteProjectCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new DeleteProjectCommand(5);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        _mockProjectRepo.Verify(r => r.Delete(It.IsAny<Project>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CanCreateTaskInOwnProject_Successfully()
    {
        // Arrange
        const string userId = "user-123";
        _mockCurrentUserService.Setup(u => u.UserId).Returns(userId);
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var ownProject = new Project { Id = 10, Name = "Own Project", UserId = userId };
        _mockProjectRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(ownProject);

        var handler = new CreateTaskCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new CreateTaskCommand("New Task", "Task in own project", TaskPriority.Medium, TaskStatus.Todo, null, 10);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(userId, result.Data!.UserId);
        _mockTaskRepo.Verify(r => r.AddAsync(It.IsAny<TaskEntity>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CannotCreateTaskInAnotherUsersProject_ThrowsForbiddenException()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("user-1");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var anotherUserProject = new Project { Id = 10, Name = "Other User Project", UserId = "user-2" };
        _mockProjectRepo.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(anotherUserProject);

        var handler = new CreateTaskCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new CreateTaskCommand("New Task", "Unauthorized insertion", TaskPriority.Medium, TaskStatus.Todo, null, 10);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        Assert.Contains("cannot create tasks in another user's project", ex.Message);
        _mockTaskRepo.Verify(r => r.AddAsync(It.IsAny<TaskEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CannotModifyAnotherUsersTask_ThrowsForbiddenException()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("user-1");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var task = new TaskEntity("Other Task", "Desc", TaskPriority.Medium, 1, null, "user-2")
        {
            Id = 20,
            Project = new Project { Id = 1, UserId = "user-2" }
        };

        _mockTaskRepo.Setup(r => r.GetTaskWithDetailsAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(task);
        _mockProjectRepo.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new UpdateTaskCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new UpdateTaskCommand(20, "Tampered Title", "Tampered Desc", TaskPriority.High, TaskStatus.Todo, null, 1);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        _mockTaskRepo.Verify(r => r.Update(It.IsAny<TaskEntity>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CannotModifyAnotherUsersTaskStatus_ThrowsForbiddenException()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("user-1");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var task = new TaskEntity("Other Task", "Desc", TaskPriority.Medium, 1, null, "user-2")
        {
            Id = 25,
            Project = new Project { Id = 1, UserId = "user-2" }
        };

        _mockTaskRepo.Setup(r => r.GetTaskWithDetailsAsync(25, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var handler = new UpdateTaskStatusCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new UpdateTaskStatusCommand(25, TaskStatus.InProgress);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        _mockTaskRepo.Verify(r => r.Update(It.IsAny<TaskEntity>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task Admin_CanModifyAnotherUsersTaskStatus_Successfully()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("admin-user");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(true);

        var task = new TaskEntity("User Task", "Desc", TaskPriority.Medium, 1, null, "user-2")
        {
            Id = 30,
            Project = new Project { Id = 1, UserId = "user-2" }
        };

        _mockTaskRepo.Setup(r => r.GetTaskWithDetailsAsync(30, It.IsAny<CancellationToken>())).ReturnsAsync(task);

        var handler = new UpdateTaskStatusCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new UpdateTaskStatusCommand(30, TaskStatus.InProgress);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal(TaskStatus.InProgress, task.Status);
        _mockTaskRepo.Verify(r => r.Update(task), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CanAddComment_WithCurrentUserInfo()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("user-1");
        _mockCurrentUserService.Setup(u => u.UserName).Returns("alice");

        _mockTaskRepo.Setup(r => r.ExistsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var handler = new CreateCommentCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new CreateCommentCommand(1, "Great job on this task!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        Assert.Equal("alice", result.Data!.Author);
        _mockCommentRepo.Verify(r => r.AddAsync(It.Is<Comment>(c => c.UserId == "user-1" && c.Author == "alice"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task User_CannotDeleteAnotherUsersComment_ThrowsForbiddenException()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("user-1");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(false);

        var comment = new Comment { Id = 100, TaskId = 1, Content = "Hello", Author = "bob", UserId = "user-2" };
        _mockCommentRepo.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        var handler = new DeleteCommentCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new DeleteCommentCommand(100);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(command, CancellationToken.None));
        _mockCommentRepo.Verify(r => r.Delete(It.IsAny<Comment>()), Times.Never);
    }

    [Fact]
    public async System.Threading.Tasks.Task Admin_CanDeleteAnyComment_Successfully()
    {
        // Arrange
        _mockCurrentUserService.Setup(u => u.UserId).Returns("admin");
        _mockCurrentUserService.Setup(u => u.IsAdmin).Returns(true);

        var comment = new Comment { Id = 100, TaskId = 1, Content = "Hello", Author = "bob", UserId = "user-2" };
        _mockCommentRepo.Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(comment);

        var handler = new DeleteCommentCommandHandler(_mockUnitOfWork.Object, _mockCurrentUserService.Object);
        var command = new DeleteCommentCommand(100);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.True(result.Succeeded);
        _mockCommentRepo.Verify(r => r.Delete(comment), Times.Once);
    }
}
