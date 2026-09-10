using Domain.Common;

namespace Domain.Entities;

public class Project : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<Task> Tasks { get; set; } = new List<Task>();
}
