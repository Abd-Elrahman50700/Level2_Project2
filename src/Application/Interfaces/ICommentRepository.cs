using Domain.Entities;

namespace Application.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    System.Threading.Tasks.Task<IReadOnlyList<Comment>> GetCommentsByTaskIdAsync(int taskId, CancellationToken cancellationToken = default);
}
