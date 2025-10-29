namespace BuildingBlocks.Messaging.RabbitMQ.Options;

public sealed class RabbitMQOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public int RetryCount { get; set; } = 3;
    public int RequestedHeartbeat { get; set; } = 60;
    public int ConnectionTimeout { get; set; } = 30;
    public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(5);
    public string ExchangeName { get; set; } = "event_bus";
    public string QueueName { get; set; } = string.Empty;
}
