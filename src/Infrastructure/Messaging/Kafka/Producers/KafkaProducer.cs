using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Producers;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaOptions> options, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        
        var config = new ProducerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            ClientId = options.Value.Producer.ClientId,
            Acks = Enum.TryParse<Acks>(options.Value.Producer.Acks, out var acks) ? acks : Confluent.Kafka.Acks.All
        };

        _producer = new ProducerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka Producer Error: {Reason}", e.Reason))
            .Build();
    }

    public async Task ProduceAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        await ProduceAsync(Guid.NewGuid().ToString(), message, topic, cancellationToken);
    }

    public async Task ProduceAsync<TKey, TMessage>(string topic, TKey key, TMessage message, CancellationToken cancellationToken = default) where TMessage : class
    {
        try
        {
            var kafkaMessage = new Message<string, string>
            {
                Key = key?.ToString() ?? Guid.NewGuid().ToString(),
                Value = JsonSerializer.Serialize(message)
            };

            var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
            _logger.LogInformation("Delivered message to {TopicPartitionOffset}", result.TopicPartitionOffset);
        }
        catch (ProduceException<string, string> e)
        {
            _logger.LogError(e, "Delivery failed: {Reason}", e.Error.Reason);
            throw;
        }
    }

    public void Dispose()
    {
        _producer?.Flush(TimeSpan.FromSeconds(10));
        _producer?.Dispose();
    }
}
