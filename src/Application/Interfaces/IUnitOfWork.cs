namespace Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IProjectRepository Projects { get; }
    ITaskRepository Tasks { get; }
    ICommentRepository Comments { get; }

    System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    System.Threading.Tasks.Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
