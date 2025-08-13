namespace EduShpere.Shared;
public class NotFoundException : Exception
{
    public NotFoundException() : base("NotFound")
    {

    }
    public NotFoundException(string message) : base(message)
    {

    }
    public NotFoundException(string message, Exception inner) : base(message, inner)
    {

    }
}
