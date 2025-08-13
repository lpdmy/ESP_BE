using System.Net;

namespace EduShpere.Application;
public class ResponseDto
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public ResponseDto(string message, int statusCode = (int)HttpStatusCode.OK)
    {
        Message = message;
        StatusCode = statusCode;
    }
}
