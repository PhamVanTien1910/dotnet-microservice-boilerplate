using System.Net.Sockets;
using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Exceptions;

namespace BuildingBlocks.Messaging.RabbitMQ.Connections;

public class RabbitMQConnection : IRabbitMQConnection
{
    private readonly IConnectionFactory _connectionFactory;
    private readonly RabbitMQOptions _options;
    private readonly ILogger<RabbitMQConnection> _logger;
    private IConnection _connection;
    private bool _disposed;

    public RabbitMQConnection(IOptions<RabbitMQOptions> options, ILogger<RabbitMQConnection> logger, IConnectionFactory connectionFactory)
    {
        _options = options.Value; // Extract the actual options object
        _logger = logger;
        _connection = null!;
        _connectionFactory = connectionFactory;
    }

    public bool IsConnected => _connection != null && _connection.IsOpen && !_disposed;

    public async Task<IChannel> CreateChannelAsync()
    {
        if (!IsConnected)
        {
            throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
        }

        return await _connection!.CreateChannelAsync();
    }

    public async Task<bool> TryConnectAsync()
    {
        _logger.LogInformation("Trying to connect to RabbitMQ...");
        var policy = Policy.Handle<SocketException>()
            .Or<BrokerUnreachableException>()
            .WaitAndRetryAsync(_options.RetryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
            {
                    _logger.LogWarning(ex, "RabbitMQ Client could not connect after {TimeOut}s ({ExceptionMessage})", $"{time.TotalSeconds:n1}", ex.Message);
            });

        await policy.ExecuteAsync(async () =>
        {
            _connection = await _connectionFactory.CreateConnectionAsync();
        });

        if (IsConnected)
        {
            _connection.ConnectionShutdownAsync += OnConnectionShutdownAsync;
            _connection.CallbackExceptionAsync += OnCallbackExceptionAsync;
            _connection.ConnectionBlockedAsync += OnConnectionBlockedAsync;

            _logger.LogInformation("RabbitMQ Client acquired a persistent connection to \'{HostName}\' and is subscribed to failure events", _connection.Endpoint.HostName);

            return true;
        }
        else
        {
            _logger.LogCritical("FATAL ERROR: RabbitMQ connections could not be created and opened");
            return false;
        }
    }

    private async Task OnConnectionBlockedAsync(object? sender, ConnectionBlockedEventArgs e)
    {
        if (_disposed) return;

        _logger.LogWarning("A RabbitMQ connection is blocked. Trying to re-connect...");

        await TryConnectAsync();
    }

    private async Task OnCallbackExceptionAsync(object? sender, CallbackExceptionEventArgs e)
    {
        if (_disposed) return;

        _logger.LogWarning("A RabbitMQ connection throw exception. Trying to re-connect...");

        await TryConnectAsync();
    }

    private async Task OnConnectionShutdownAsync(object? sender, ShutdownEventArgs reason)
    {
        if (_disposed) return;

        _logger.LogWarning("A RabbitMQ connection is on shutdown. Trying to re-connect...");

        await TryConnectAsync();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        try
        {
            if (_connection != null)
            {
                _connection.ConnectionShutdownAsync -= OnConnectionShutdownAsync;
                _connection.CallbackExceptionAsync -= OnCallbackExceptionAsync;
                _connection.ConnectionBlockedAsync -= OnConnectionBlockedAsync;
            }
            _connection?.Dispose();
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "An error occurred while disposing RabbitMQ connection.");
        }
    }
}
