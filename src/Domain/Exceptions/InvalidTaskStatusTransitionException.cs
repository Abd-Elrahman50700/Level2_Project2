using Domain.Enums;

namespace Domain.Exceptions;

public class InvalidTaskStatusTransitionException : DomainException
{
    public TaskStatus FromStatus { get; }
    public TaskStatus ToStatus { get; }

    public InvalidTaskStatusTransitionException(TaskStatus fromStatus, TaskStatus toStatus)
        : base($"Cannot transition task status from '{fromStatus}' to '{toStatus}'.")
    {
        FromStatus = fromStatus;
        ToStatus = toStatus;
    }
}
