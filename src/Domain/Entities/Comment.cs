using Domain.Common;

namespace Domain.Entities;

public class Comment : BaseEntity
{
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;

    public string? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;
}
