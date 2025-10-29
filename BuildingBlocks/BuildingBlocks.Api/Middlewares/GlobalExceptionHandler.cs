using BuildingBlocks.Domain.Exceptions;
using BuildingBlocks.MediatR.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Api.Middlewares;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {

        var problemDetails = CreateProblemDetails(httpContext, exception);

        httpContext.Response.StatusCode = problemDetails.Status!.Value;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblemDetails(HttpContext httpContext, Exception exception)
    {
        return exception switch
        {
            FluentValidationException validationException => CreateValidationProblemDetails(httpContext, validationException.Errors),

            BaseException domainException => CreateDomainProblemDetails(httpContext, domainException),

            _ => new ProblemDetails
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "An error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "We are working to resolve the issue. Please try again later.",
                Instance = httpContext.Request.Path
            }
        };
    }

    private static ProblemDetails CreateValidationProblemDetails(
        HttpContext httpContext,
        IDictionary<string, string[]> errors)
    {
        var problemDetails = new ProblemDetails
        {
            Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Instance = httpContext.Request.Path
        };

        problemDetails.Extensions["errors"] = errors;

        return problemDetails;
    }

    private static ProblemDetails CreateDomainProblemDetails(HttpContext httpContext, BaseException domainException)
    {
        var statusCode = (int)domainException.StatusCode;
        var type = GetStandardProblemType(statusCode);

        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = domainException.Title,
            Status = statusCode,
            Detail = domainException.Message,
            Instance = httpContext.Request.Path
        };

        return problemDetails;
    }

    private static string GetStandardProblemType(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",  // Bad Request
        401 => "https://tools.ietf.org/html/rfc7235#section-3.1",   // Unauthorized
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3", // Forbidden
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4", // Not Found
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8", // Conflict
        422 => "https://tools.ietf.org/html/rfc4918#section-11.2",  // Unprocessable Entity
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1", // Internal Server Error
        _ => "about:blank" // Default RFC 7807 type for unknown status codes
    };
}