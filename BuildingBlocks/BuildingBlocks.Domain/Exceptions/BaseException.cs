using System.Net;

namespace BuildingBlocks.Domain.Exceptions;
public class BaseException(string message, string title, HttpStatusCode statusCode) 
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; set; } = statusCode;
    public string Title { get; set; } = title;
}