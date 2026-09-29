namespace Application.Common.Exceptions;

public class ForbiddenException : Exception
{
    public ForbiddenException()
        : base("You do not have permission to access or modify this resource.")
    {
    }

    public ForbiddenException(string message)
        : base(message)
    {
    }
}
