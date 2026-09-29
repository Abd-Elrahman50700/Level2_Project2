using Application.Features.Comments.Commands;
using Application.Features.Comments.Validators;
using Application.Features.Projects.Commands;
using Application.Features.Projects.Validators;
using Application.Features.Tasks.Commands;
using Application.Features.Tasks.Validators;
using Domain.Enums;
using Xunit;

namespace UnitTests.Application;

public class CommandValidationTests
{
    // --- CreateProjectCommandValidator Tests ---
    [Fact]
    public void CreateProjectValidator_ShouldPass_ForValidInput()
    {
        var validator = new CreateProjectCommandValidator();
        var command = new CreateProjectCommand("Project Alpha", "Description of alpha project");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateProjectValidator_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new CreateProjectCommandValidator();
        var command = new CreateProjectCommand("", "Description");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public void CreateProjectValidator_ShouldFail_WhenNameExceeds200Characters()
    {
        var validator = new CreateProjectCommandValidator();
        var command = new CreateProjectCommand(new string('a', 201), "Description");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public void CreateProjectValidator_ShouldFail_WhenDescriptionExceeds1000Characters()
    {
        var validator = new CreateProjectCommandValidator();
        var command = new CreateProjectCommand("Valid Name", new string('d', 1001));

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.ErrorMessage.Contains("1000"));
    }

    // --- UpdateProjectCommandValidator Tests ---
    [Fact]
    public void UpdateProjectValidator_ShouldPass_ForValidInput()
    {
        var validator = new UpdateProjectCommandValidator();
        var command = new UpdateProjectCommand(1, "Updated Project", "Updated Description");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateProjectValidator_ShouldFail_WhenIdIsZeroOrNegative()
    {
        var validator = new UpdateProjectCommandValidator();
        var command = new UpdateProjectCommand(0, "Valid Name", "Description");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void UpdateProjectValidator_ShouldFail_WhenNameIsEmpty()
    {
        var validator = new UpdateProjectCommandValidator();
        var command = new UpdateProjectCommand(1, "", "Description");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Name");
    }

    // --- CreateTaskCommandValidator Tests ---
    [Fact]
    public void CreateTaskValidator_ShouldPass_ForValidInput()
    {
        var validator = new CreateTaskCommandValidator();
        var command = new CreateTaskCommand("New Task", "Task details", TaskPriority.High, TaskStatus.Todo, null, 1);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateTaskValidator_ShouldFail_WhenTitleIsEmpty()
    {
        var validator = new CreateTaskCommandValidator();
        var command = new CreateTaskCommand("", "Details", TaskPriority.Medium, TaskStatus.Todo, null, 1);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Title");
    }

    [Fact]
    public void CreateTaskValidator_ShouldFail_WhenStatusIsNotTodo()
    {
        var validator = new CreateTaskCommandValidator();
        var command = new CreateTaskCommand("Title", "Details", TaskPriority.Medium, TaskStatus.InProgress, null, 1);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status" && e.ErrorMessage.Contains("Todo"));
    }

    [Fact]
    public void CreateTaskValidator_ShouldFail_WhenProjectIdIsInvalid()
    {
        var validator = new CreateTaskCommandValidator();
        var command = new CreateTaskCommand("Title", "Details", TaskPriority.Medium, TaskStatus.Todo, null, 0);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "ProjectId");
    }

    [Fact]
    public void CreateTaskValidator_ShouldFail_WhenPriorityIsOutOfRange()
    {
        var validator = new CreateTaskCommandValidator();
        var command = new CreateTaskCommand("Title", "Details", (TaskPriority)99, TaskStatus.Todo, null, 1);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Priority");
    }

    // --- UpdateTaskCommandValidator Tests ---
    [Fact]
    public void UpdateTaskValidator_ShouldPass_ForValidInput()
    {
        var validator = new UpdateTaskCommandValidator();
        var command = new UpdateTaskCommand(1, "Updated Task", "Updated details", TaskPriority.Critical, TaskStatus.InProgress, null, 1);

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void UpdateTaskValidator_ShouldFail_WhenIdIsInvalid()
    {
        var validator = new UpdateTaskCommandValidator();
        var command = new UpdateTaskCommand(0, "Updated Task", "Details", TaskPriority.Low, TaskStatus.Todo, null, 1);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Id");
    }

    [Fact]
    public void UpdateTaskValidator_ShouldFail_WhenStatusIsInvalid()
    {
        var validator = new UpdateTaskCommandValidator();
        var command = new UpdateTaskCommand(1, "Updated Task", "Details", TaskPriority.Low, (TaskStatus)999, null, 1);

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Status");
    }

    // --- CreateCommentCommandValidator Tests ---
    [Fact]
    public void CreateCommentValidator_ShouldPass_ForValidInput()
    {
        var validator = new CreateCommentCommandValidator();
        var command = new CreateCommentCommand(1, "This is a great task", "reviewer");

        var result = validator.Validate(command);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void CreateCommentValidator_ShouldFail_WhenTaskIdIsZeroOrNegative()
    {
        var validator = new CreateCommentCommandValidator();
        var command = new CreateCommentCommand(0, "Comment content");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "TaskId");
    }

    [Fact]
    public void CreateCommentValidator_ShouldFail_WhenContentIsEmpty()
    {
        var validator = new CreateCommentCommandValidator();
        var command = new CreateCommentCommand(1, "");

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Content");
    }

    [Fact]
    public void CreateCommentValidator_ShouldFail_WhenAuthorExceeds100Characters()
    {
        var validator = new CreateCommentCommandValidator();
        var command = new CreateCommentCommand(1, "Comment content", new string('x', 101));

        var result = validator.Validate(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Author");
    }
}
