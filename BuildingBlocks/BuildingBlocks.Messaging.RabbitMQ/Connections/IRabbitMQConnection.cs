using RabbitMQ.Client;

namespace BuildingBlocks.Messaging.RabbitMQ.Connections;

public interface IRabbitMQConnection : IDisposable
{
    bool IsConnected { get; }

    Task<IChannel> CreateChannelAsync();

    Task<bool> TryConnectAsync();
}
