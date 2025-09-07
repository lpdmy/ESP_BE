using System.Net;

namespace EduShpere.Application;
public class ResponseDto<T>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public T Data { get; set; }

    public ResponseDto(T data, string message = "", int statusCode = (int)HttpStatusCode.OK)
    {
        Data = data;
        Message = message;
        StatusCode = statusCode;
    }
}
