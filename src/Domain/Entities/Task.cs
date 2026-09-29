using Domain.Common;
using Domain.Enums;
using Domain.Exceptions;

namespace Domain.Entities;

public class Task : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskStatus Status { get; private set; } = TaskStatus.Todo;
    public DateTime? DueDate { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public Task()
    {
    }

    public Task(string title, string? description, TaskPriority priority, int projectId, DateTime? dueDate = null, string? userId = null)
    {
        Title = title;
        Description = description;
        Priority = priority;
        ProjectId = projectId;
        DueDate = dueDate;
        UserId = userId;
        Status = TaskStatus.Todo;
    }

    public bool CanTransitionTo(TaskStatus newStatus)
    {
        return (Status, newStatus) switch
        {
            (TaskStatus.Todo, TaskStatus.InProgress) => true,
            (TaskStatus.Todo, TaskStatus.Cancelled) => true,
            (TaskStatus.InProgress, TaskStatus.Completed) => true,
            (TaskStatus.InProgress, TaskStatus.Cancelled) => true,
            _ => false
        };
    }

    public void UpdateStatus(TaskStatus newStatus)
    {
        if (!CanTransitionTo(newStatus))
        {
            throw new InvalidTaskStatusTransitionException(Status, newStatus);
        }

        Status = newStatus;
    }

    public void ChangeStatus(TaskStatus newStatus) => UpdateStatus(newStatus);
}
