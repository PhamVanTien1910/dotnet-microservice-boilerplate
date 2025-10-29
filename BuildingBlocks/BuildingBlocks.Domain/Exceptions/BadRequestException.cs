using System.Net;

namespace BuildingBlocks.Domain.Exceptions;
public class BadRequestException(string message) 
    : BaseException(message, "BadRequest", HttpStatusCode.BadRequest)
{
}