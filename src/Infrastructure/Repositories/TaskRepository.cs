using Application.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using TaskEntity = Domain.Entities.Task;

namespace Infrastructure.Repositories;

public class TaskRepository : Repository<TaskEntity>, ITaskRepository
{
    public TaskRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<TaskEntity>> GetTasksByProjectIdAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .Where(t => t.ProjectId == projectId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<TaskEntity?> GetTaskWithDetailsAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<TaskEntity>> GetTasksWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(t => t.Project)
            .Include(t => t.Comments)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
