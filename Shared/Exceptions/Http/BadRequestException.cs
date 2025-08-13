namespace EduShpere.Shared;
public class BadRequestException : Exception
{
    public BadRequestException() : base("BadRequest")
    {

    }
    public BadRequestException(string message) : base(message)
    {

    }
    public BadRequestException(string message, Exception inner) : base(message, inner)
    {

    }
}

