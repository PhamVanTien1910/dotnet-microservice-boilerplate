using System.Net;

namespace BuildingBlocks.Domain.Exceptions;

public class ConflictException(string message) 
        : BaseException(message, "Conflict", HttpStatusCode.Conflict)
{
}