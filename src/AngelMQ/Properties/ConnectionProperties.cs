namespace AngelMQ.Properties;

public sealed class ConnectionProperties
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public ushort MaxRetryAttempts { get; set; } = 5;
    public ushort DelayMultiplier { get; set; } = 2;
    public ushort ConsumerDispatchConcurrency { get; set; } = 1;
}
