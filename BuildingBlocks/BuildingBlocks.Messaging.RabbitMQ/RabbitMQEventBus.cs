using BuildingBlocks.Messaging.RabbitMQ.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text.Json;
using Polly;
using RabbitMQ.Client.Exceptions;
using BuildingBlocks.Application.Messaging.Abstractions;
using BuildingBlocks.Messaging.RabbitMQ.Connections;
using BuildingBlocks.Application.Messaging.Events;
using System.Net.Sockets;
using System.Text;

namespace BuildingBlocks.Messaging.RabbitMQ;

public class RabbitMQEventBus : IEventBus, IDisposable
{
    private readonly RabbitMQOptions _options;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<RabbitMQEventBus> _logger;
    private readonly IRabbitMQConnection _connection;
    private readonly ConcurrentDictionary<string, Type> _eventTypes;
    private readonly ConcurrentDictionary<string, List<Type>> _handlers;
    private readonly ConcurrentDictionary<string, string> _consumerTags;
    private IChannel? _consumerChannel;
    private AsyncEventingBasicConsumer? _consumer;
    private readonly IAsyncPolicy _retryPolicy;

    public RabbitMQEventBus(
        IOptions<RabbitMQOptions> options,
        IServiceProvider serviceProvider,
        ILogger<RabbitMQEventBus> logger,
        IRabbitMQConnection rabbitMQConnection)
    {
        _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _connection = rabbitMQConnection ?? throw new ArgumentNullException(nameof(rabbitMQConnection));
        _eventTypes = new ConcurrentDictionary<string, Type>();
        _handlers = new ConcurrentDictionary<string, List<Type>>();
        _consumerTags = new ConcurrentDictionary<string, string>();
        _retryPolicy = CreateRetryPolicy();
    }

    public async Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(@event);
        
        if (!_connection.IsConnected)
        {
            await _connection.TryConnectAsync();
        }

        var eventName = @event.GetType().Name;
        if (string.IsNullOrEmpty(eventName))
        {
            throw new ArgumentException("Event type name cannot be null or empty", nameof(@event));
        }

        await _retryPolicy.ExecuteAsync(async () =>
        {
            using var channel = await _connection.CreateChannelAsync();
            await DeclareExchangeAsync(channel);

            var body = JsonSerializer.SerializeToUtf8Bytes(@event, @event.GetType());
            var properties = CreateMessageProperties(@event, eventName);

            await channel.BasicPublishAsync(
                exchange: _options.ExchangeName,
                routingKey: eventName,
                mandatory: true,
                basicProperties: properties,
                body: body);

            _logger.LogInformation("Published event {EventName} with ID {EventId}", eventName, @event.Id);
        });
    }

    public async Task SubscribeAsync<TEvent, THandler>(CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        if (string.IsNullOrEmpty(eventName))
        {
            throw new ArgumentException("Event type name cannot be null or empty", nameof(TEvent));
        }

        _logger.LogInformation("Subscribing to event {EventName} with {EventHandler}", eventName, handlerType.Name);

        RegisterEventHandler(eventName, typeof(TEvent), handlerType);

        if (!_connection.IsConnected)
        {
            await _connection.TryConnectAsync();
        }

        await EnsureConsumerChannelAsync();
        await SetupQueueAndConsumerAsync(eventName);
    }

    public void Unsubscribe<TEvent, THandler>()
        where TEvent : IntegrationEvent
        where THandler : IIntegrationEventHandler<TEvent>
    {
        var eventName = typeof(TEvent).Name;
        var handlerType = typeof(THandler);

        if (_handlers.TryGetValue(eventName, out var handlers))
        {
            handlers.Remove(handlerType);
            if (!handlers.Any())
            {
                _handlers.TryRemove(eventName, out _);
                _eventTypes.TryRemove(eventName, out _);
            }
        }

        _logger.LogInformation("Unsubscribed from event {EventName} with handler {HandlerName}",
            eventName, handlerType.Name);
    }

    private void RegisterEventHandler(string eventName, Type eventType, Type handlerType)
    {
        _eventTypes.TryAdd(eventName, eventType);

        if (!_handlers.ContainsKey(eventName))
        {
            _handlers[eventName] = new List<Type>();
        }

        if (!_handlers[eventName].Contains(handlerType))
        {
            _handlers[eventName].Add(handlerType);
        }
    }

    private async Task EnsureConsumerChannelAsync()
    {
        if (_consumerChannel == null)
        {
            _consumerChannel = await _connection.CreateChannelAsync();
            await DeclareExchangeAsync(_consumerChannel);

            _consumer = new AsyncEventingBasicConsumer(_consumerChannel);
            _consumer.ReceivedAsync += Consumer_Received;
        }
    }

    private async Task SetupQueueAndConsumerAsync(string eventName)
    {
        var queueName = $"{_options.QueueName}.{eventName}";
        
        await _consumerChannel!.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false);

        await _consumerChannel.QueueBindAsync(
            queue: queueName,
            exchange: _options.ExchangeName,
            routingKey: eventName);

        var consumerTag = await _consumerChannel.BasicConsumeAsync(
            queue: queueName,
            autoAck: false,
            consumer: _consumer!);

        _consumerTags[eventName] = consumerTag;

        _logger.LogInformation("Successfully subscribed to event {EventName} on queue {QueueName}", eventName, queueName);
    }

    private async Task DeclareExchangeAsync(IChannel channel)
    {
        await channel.ExchangeDeclareAsync(
            exchange: _options.ExchangeName,
            type: ExchangeType.Direct,
            durable: true);
    }

    private static BasicProperties CreateMessageProperties(IntegrationEvent @event, string eventName)
    {
        return new BasicProperties
        {
            DeliveryMode = DeliveryModes.Persistent,
            MessageId = @event.Id.ToString(),
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
            Type = eventName
        };
    }

    private async Task Consumer_Received(object sender, BasicDeliverEventArgs eventArgs)
    {
        var eventName = eventArgs.RoutingKey;
        var message = Encoding.UTF8.GetString(eventArgs.Body.Span);

        try
        {
            await ProcessEventAsync(eventName, message);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error processing message \"{Message}\"", message);
        }

        if (_consumerChannel != null)
        {
            await _consumerChannel.BasicAckAsync(eventArgs.DeliveryTag, multiple: false);
        }
    }

    private async Task ProcessEventAsync(string eventName, string message)
    {
        _logger.LogTrace("Processing RabbitMQ event: {EventName}", eventName);

        if (!_handlers.TryGetValue(eventName, out var handlerTypes) || 
            !_eventTypes.TryGetValue(eventName, out var eventType))
        {
            _logger.LogWarning("No subscription for RabbitMQ event: {EventName}", eventName);
            return;
        }

        var @event = JsonSerializer.Deserialize(message, eventType, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (@event == null)
        {
            _logger.LogWarning("Failed to deserialize event {EventName}", eventName);
            return;
        }

        using var scope = _serviceProvider.CreateScope();
        var tasks = handlerTypes.Select(handlerType => 
            ProcessEventHandlerAsync(scope.ServiceProvider, handlerType, @event, eventName));

        await Task.WhenAll(tasks);
        _logger.LogDebug("Successfully processed event {EventName}", eventName);
    }

    private static async Task ProcessEventHandlerAsync(IServiceProvider serviceProvider, Type handlerType, object @event, string eventName)
    {
        try
        {
            var handler = serviceProvider.GetService(handlerType);
            if (handler == null)
            {
                // Try to resolve by interface type if concrete type fails
                var eventType = @event.GetType();
                var interfaceType = typeof(IIntegrationEventHandler<>).MakeGenericType(eventType);
                handler = serviceProvider.GetService(interfaceType);
                
                if (handler == null)
                {
                    var logger = serviceProvider.GetService<ILogger<RabbitMQEventBus>>();
                    logger?.LogWarning("No handler found for event {EventName} with handler type {HandlerType}", eventName, handlerType.Name);
                    return;
                }
            }

            var handleMethod = handler.GetType().GetMethod("HandleAsync");
            if (handleMethod?.Invoke(handler, new[] { @event, CancellationToken.None }) is Task task)
            {
                await task;
            }
        }
        catch (Exception ex)
        {
            var logger = serviceProvider.GetService<ILogger<RabbitMQEventBus>>();
            logger?.LogError(ex, "Error processing event {EventName} with handler {HandlerType}", eventName, handlerType.Name);
        }
    }

    private IAsyncPolicy CreateRetryPolicy()
    {
        return Policy.Handle<BrokerUnreachableException>()
            .Or<SocketException>()
            .WaitAndRetryAsync(
                _options.RetryCount, 
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), 
                (ex, time) =>
                {
                    _logger.LogWarning(ex, "Could not publish event after {TimeOut}s", time.TotalSeconds);
                });
    }

    public void Dispose()
    {
        _consumer = null;
        _consumerTags.Clear();
        _consumerChannel?.Dispose();
    }
}
