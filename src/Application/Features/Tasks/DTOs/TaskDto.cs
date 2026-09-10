using Domain.Enums;

namespace Application.Features.Tasks.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CommentCount { get; set; }
}

public class CreateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
}

public class UpdateTaskDto
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; }
    public TaskItemStatus Status { get; set; }
    public DateTime? DueDate { get; set; }
    public int ProjectId { get; set; }
}
