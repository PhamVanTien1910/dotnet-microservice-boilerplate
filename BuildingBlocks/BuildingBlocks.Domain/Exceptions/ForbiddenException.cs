using System.Net;

namespace BuildingBlocks.Domain.Exceptions;
public class ForbiddenException(string message) 
    : BaseException(message, "Forbidden", HttpStatusCode.Forbidden)
{
}
