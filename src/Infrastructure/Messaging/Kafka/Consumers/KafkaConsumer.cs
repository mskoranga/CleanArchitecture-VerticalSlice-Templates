using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Configuration;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace CleanArchitecture.VerticalSlice.Infrastructure.Messaging.Kafka.Consumers;

public class KafkaConsumer<TMessage> : IKafkaConsumer<TMessage>, IDisposable where TMessage : class
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumer<TMessage>> _logger;

    public KafkaConsumer(IOptions<KafkaOptions> options, ILogger<KafkaConsumer<TMessage>> logger)
    {
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = options.Value.BootstrapServers,
            GroupId = options.Value.Consumer.GroupId,
            AutoOffsetReset = Enum.TryParse<AutoOffsetReset>(options.Value.Consumer.AutoOffsetReset, out var autoOffsetReset) 
                ? autoOffsetReset 
                : Confluent.Kafka.AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogError("Kafka Consumer Error: {Reason}", e.Reason))
            .Build();
    }

    public async Task StartConsumeAsync(string topic, Func<TMessage, Task> messageHandler, CancellationToken cancellationToken = default)
    {
        _consumer.Subscribe(topic);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);

                    if (consumeResult?.Message == null) continue;

                    var message = JsonSerializer.Deserialize<TMessage>(consumeResult.Message.Value);
                    if (message != null)
                    {
                        await messageHandler(message);
                    }

                    _consumer.Commit(consumeResult);
                }
                catch (ConsumeException e)
                {
                    _logger.LogError(e, "Consume error: {Reason}", e.Error.Reason);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Closing consumer.");
            _consumer.Close();
        }
    }

    public void Dispose()
    {
        _consumer?.Dispose();
    }
}
