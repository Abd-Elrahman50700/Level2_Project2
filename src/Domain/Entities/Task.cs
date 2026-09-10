using Domain.Common;
using Domain.Enums;

namespace Domain.Entities;

public class Task : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public DateTime? DueDate { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}
