namespace EduShpere.Shared;
public class ConflictException : Exception
{
    public ConflictException() : base("Conflict")
    {

    }
    public ConflictException(string message) : base(message)
    {

    }
    public ConflictException(string message, Exception inner) : base(message, inner)
    {

    }
}

