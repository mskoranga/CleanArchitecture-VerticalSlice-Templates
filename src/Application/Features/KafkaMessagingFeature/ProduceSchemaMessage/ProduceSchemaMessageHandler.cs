namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemaMessage;

using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;
using Microsoft.Extensions.Logging;

public sealed record ProduceSchemaMessageRequest<T>(
    string Topic,
    string? Key,
    string Schema,
    string SchemaVersion,
    T Payload,
    Dictionary<string, string>? Metadata = null) where T : class;

public sealed record ProduceSchemaMessageResponse(
    string MessageId,
    string Topic,
    string Schema,
    DateTime ProducedAt);

public sealed class ProduceOrderEventHandler(
    IKafkaProducer _kafkaProducer,
    ILogger<ProduceOrderEventHandler> _logger)
    : IHandler<ProduceSchemaMessageRequest<OrderCreatedEvent>, Result<ProduceSchemaMessageResponse>>
{
    public async Task<Result<ProduceSchemaMessageResponse>> HandleAsync(
        ProduceSchemaMessageRequest<OrderCreatedEvent> request,
        CancellationToken cancellationToken)
    {
        try
        {
            var messageId = Guid.CreateVersion7().ToString();
            var key = request.Key ?? request.Payload.OrderId;

            var schemaMessage = new SchemaMessage<OrderCreatedEvent>(
                MessageId: messageId,
                Schema: request.Schema,
                SchemaVersion: request.SchemaVersion,
                Payload: request.Payload,
                Timestamp: DateTime.UtcNow,
                Metadata: request.Metadata ?? new Dictionary<string, string>
                {
                    { "Source", "KafkaMessagingFeature" },
                    { "Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development" }
                });

            await _kafkaProducer.ProduceAsync(request.Topic, key, schemaMessage, cancellationToken);

            _logger.LogInformation(
                "Successfully produced schema-based message {MessageId} with schema {Schema} v{Version} to topic {Topic}",
                messageId,
                request.Schema,
                request.SchemaVersion,
                request.Topic);

            return Result.Success(new ProduceSchemaMessageResponse(
                messageId,
                request.Topic,
                request.Schema,
                DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Failed to produce schema-based message with schema {Schema} to topic {Topic}", 
                request.Schema, 
                request.Topic);
            return Result.Failure<ProduceSchemaMessageResponse>(
                KafkaErrors.ProduceFailed(request.Topic, ex.Message));
        }
    }
}

public sealed class ProduceUserEventHandler(
    IKafkaProducer _kafkaProducer,
    ILogger<ProduceUserEventHandler> _logger)
    : IHandler<ProduceSchemaMessageRequest<UserRegisteredEvent>, Result<ProduceSchemaMessageResponse>>
{
    public async Task<Result<ProduceSchemaMessageResponse>> HandleAsync(
        ProduceSchemaMessageRequest<UserRegisteredEvent> request,
        CancellationToken cancellationToken)
    {
        try
        {
            var messageId = Guid.CreateVersion7().ToString();
            var key = request.Key ?? request.Payload.UserId;

            var schemaMessage = new SchemaMessage<UserRegisteredEvent>(
                MessageId: messageId,
                Schema: request.Schema,
                SchemaVersion: request.SchemaVersion,
                Payload: request.Payload,
                Timestamp: DateTime.UtcNow,
                Metadata: request.Metadata ?? new Dictionary<string, string>
                {
                    { "Source", "KafkaMessagingFeature" },
                    { "Environment", Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development" }
                });

            await _kafkaProducer.ProduceAsync(request.Topic, key, schemaMessage, cancellationToken);

            _logger.LogInformation(
                "Successfully produced schema-based message {MessageId} with schema {Schema} v{Version} to topic {Topic}",
                messageId,
                request.Schema,
                request.SchemaVersion,
                request.Topic);

            return Result.Success(new ProduceSchemaMessageResponse(
                messageId,
                request.Topic,
                request.Schema,
                DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to produce schema-based message with schema {Schema} to topic {Topic}",
                request.Schema,
                request.Topic);
            return Result.Failure<ProduceSchemaMessageResponse>(
                KafkaErrors.ProduceFailed(request.Topic, ex.Message));
        }
    }
}
