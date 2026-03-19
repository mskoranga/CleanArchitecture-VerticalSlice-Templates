namespace CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Configuration;

public class KafkaOptions
{
    public string BootstrapServers { get; set; } = string.Empty;
    public ProducerOptions Producer { get; set; } = new();
    public ConsumerOptions Consumer { get; set; } = new();
}

public class ProducerOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string Acks { get; set; } = "All";
}

public class ConsumerOptions
{
    public string GroupId { get; set; } = string.Empty;
    public string AutoOffsetReset { get; set; } = "Earliest";
}
