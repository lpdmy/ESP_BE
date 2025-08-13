namespace EduShpere.Shared;
public class ExternalServiceException : Exception
{
    public ExternalServiceException() : base("BadRequest")
    {

    }
    public ExternalServiceException(string message) : base(message)
    {

    }
    public ExternalServiceException(string message, Exception inner) : base(message, inner)
    {

    }
}
