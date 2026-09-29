namespace Application.Common.Exceptions;

public class ConflictException : Exception
{
    public ConflictException() 
        : base("A conflict occurred with an existing resource.")
    {
    }

    public ConflictException(string message) 
        : base(message)
    {
    }

    public ConflictException(string entityName, object key)
        : base($"Entity \"{entityName}\" ({key}) is in a conflict state or already exists.")
    {
    }
}
