using TaskEntity = Domain.Entities.Task;

namespace Application.Interfaces;

public interface ITaskRepository : IRepository<TaskEntity>
{
    System.Threading.Tasks.Task<IReadOnlyList<TaskEntity>> GetTasksByProjectIdAsync(int projectId, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<TaskEntity?> GetTaskWithDetailsAsync(int id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<TaskEntity>> GetTasksWithDetailsAsync(CancellationToken cancellationToken = default);
}
