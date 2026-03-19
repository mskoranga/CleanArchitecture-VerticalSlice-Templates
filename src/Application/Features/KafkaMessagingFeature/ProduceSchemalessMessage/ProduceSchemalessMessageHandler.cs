namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemalessMessage;

using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;
using Microsoft.Extensions.Logging;

public sealed record ProduceSchemalessMessageRequest(
    string Topic,
    string? Key,
    Dictionary<string, object> Payload);

public sealed record ProduceSchemalessMessageResponse(
    string MessageId,
    string Topic,
    DateTime ProducedAt);

public sealed class ProduceSchemalessMessageHandler(
    IKafkaProducer _kafkaProducer,
    ILogger<ProduceSchemalessMessageHandler> _logger) 
    : IHandler<ProduceSchemalessMessageRequest, Result<ProduceSchemalessMessageResponse>>
{
    public async Task<Result<ProduceSchemalessMessageResponse>> HandleAsync(
        ProduceSchemalessMessageRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var messageId = Guid.CreateVersion7().ToString();
            var key = request.Key ?? messageId;

            var message = new
            {
                MessageId = messageId,
                Key = key,
                Payload = request.Payload,
                Timestamp = DateTime.UtcNow
            };

            await _kafkaProducer.ProduceAsync(request.Topic, key, message, cancellationToken);

            _logger.LogInformation(
                "Successfully produced schemaless message {MessageId} to topic {Topic}",
                messageId,
                request.Topic);

            return Result.Success(new ProduceSchemalessMessageResponse(
                messageId,
                request.Topic,
                DateTime.UtcNow));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to produce schemaless message to topic {Topic}", request.Topic);
            return Result.Failure<ProduceSchemalessMessageResponse>(
                KafkaErrors.ProduceFailed(request.Topic, ex.Message));
        }
    }
}
