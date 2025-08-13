namespace EduShpere.Shared;
public class ForbiddenException : Exception
{
    public ForbiddenException() : base("Forbidden")
    {

    }
    public ForbiddenException(string message) : base(message)
    {

    }
    public ForbiddenException(string message, Exception inner) : base(message, inner)
    {

    }
}
