using System.Diagnostics;
using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.MediatR.Behaviors;

public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Request cannot be null");
        }

        _logger.LogInformation("Handling request: {RequestName}", typeof(TRequest).Name);

        Type myType = request.GetType();
        IList<PropertyInfo> properties = myType.GetProperties().ToList();
        foreach (var property in properties)
        {
            var value = property.GetValue(request);
            _logger.LogDebug("Property: {PropertyName}, Value: {PropertyValue}", property.Name, value);
        }

        var sw = Stopwatch.StartNew();

        var response = await next(cancellationToken);

        _logger.LogInformation("Handled request: {RequestName} in {ms} ms", typeof(TRequest).Name, sw.ElapsedMilliseconds);
        _logger.LogDebug("Response: {Response}", response);

        sw.Stop();
        return response;
    }
}