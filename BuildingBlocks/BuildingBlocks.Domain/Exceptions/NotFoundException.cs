using System.Net;

namespace BuildingBlocks.Domain.Exceptions;
public class NotFoundException(string message) 
    : BaseException(message, "NotFound", HttpStatusCode.NotFound)
{
}