using Domain.Entities;

namespace Application.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    System.Threading.Tasks.Task<Project?> GetProjectWithTasksAsync(int id, CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task<IReadOnlyList<Project>> GetProjectsWithTasksAsync(CancellationToken cancellationToken = default);
}
