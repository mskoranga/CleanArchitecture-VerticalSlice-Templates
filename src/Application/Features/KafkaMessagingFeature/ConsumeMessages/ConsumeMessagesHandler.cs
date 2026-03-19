namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ConsumeMessages;

using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public sealed record ConsumeMessagesRequest(
    string Topic,
    string ConsumerGroupId,
    bool UseSchema = false,
    int MaxMessages = 10);

public sealed record ConsumeMessagesResponse(
    string Topic,
    int MessageCount,
    List<ConsumedMessageInfo> Messages);

public sealed record ConsumedMessageInfo(
    string MessageId,
    string Key,
    object Payload,
    DateTime ConsumedAt,
    string? Schema = null);

public sealed class ConsumeSchemalessMessagesHandler(
    IKafkaConsumer<Dictionary<string, JsonElement>> _kafkaConsumer,
    ILogger<ConsumeSchemalessMessagesHandler> _logger)
    : IHandler<ConsumeMessagesRequest, Result<ConsumeMessagesResponse>>
{
    public async Task<Result<ConsumeMessagesResponse>> HandleAsync(
        ConsumeMessagesRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var consumedMessages = new List<ConsumedMessageInfo>();
            var messageCount = 0;

            await _kafkaConsumer.StartConsumeAsync(
                request.Topic,
                async message =>
                {
                    if (messageCount >= request.MaxMessages)
                    {
                        return;
                    }

                    var messageId = message.ContainsKey("MessageId")
                        ? message["MessageId"].ToString()
                        : Guid.CreateVersion7().ToString();

                    var key = message.ContainsKey("Key")
                        ? message["Key"].ToString()
                        : "unknown";

                    consumedMessages.Add(new ConsumedMessageInfo(
                        MessageId: messageId ?? Guid.CreateVersion7().ToString(),
                        Key: key ?? "unknown",
                        Payload: message,
                        ConsumedAt: DateTime.UtcNow));

                    messageCount++;

                    _logger.LogInformation(
                        "Consumed schemaless message {MessageId} from topic {Topic}",
                        messageId,
                        request.Topic);

                    await Task.CompletedTask;
                },
                cancellationToken);

            return Result.Success(new ConsumeMessagesResponse(
                request.Topic,
                consumedMessages.Count,
                consumedMessages));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to consume messages from topic {Topic}", request.Topic);
            return Result.Failure<ConsumeMessagesResponse>(
                KafkaErrors.ConsumeFailed(request.Topic, ex.Message));
        }
    }
}
