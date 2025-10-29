using System.Net;

namespace BuildingBlocks.Domain.Exceptions;
public class UnauthorizedException(string message) 
    : BaseException(message, "Unauthorized", HttpStatusCode.Unauthorized)
{
}
