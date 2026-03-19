namespace CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;

public interface IKafkaProducer
{
    Task ProduceAsync<TMessage>(string topic, TMessage message, CancellationToken cancellationToken = default) where TMessage : class;
    Task ProduceAsync<TKey, TMessage>(string topic, TKey key, TMessage message, CancellationToken cancellationToken = default) where TMessage : class;
}

public interface IKafkaConsumer<TMessage> where TMessage : class
{
    Task StartConsumeAsync(string topic, Func<TMessage, Task> messageHandler, CancellationToken cancellationToken = default);
}
