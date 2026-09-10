using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<Project?> GetProjectWithTasksAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetProjectsWithTasksAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Include(p => p.Tasks)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
