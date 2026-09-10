using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CommentRepository : Repository<Comment>, ICommentRepository
{
    public CommentRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Comment>> GetCommentsByTaskIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(c => c.TaskId == taskId)
            .OrderByDescending(c => c.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
