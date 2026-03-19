using CleanArchitecture.VerticalSlice.Domain.Abstractions.Errors;

namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature;

public static class KafkaErrors
{
    public static Error ProduceFailed(string topic, string reason) =>
        Error.Failure("Kafka.ProduceFailed", $"Failed to produce message to topic '{topic}': {reason}");

    public static Error ConsumeFailed(string topic, string reason) =>
        Error.Failure("Kafka.ConsumeFailed", $"Failed to consume message from topic '{topic}': {reason}");

    public static Error InvalidMessage(string reason) =>
        Error.Validation("Kafka.InvalidMessage", $"Invalid message: {reason}");

    public static Error TopicNotFound(string topic) =>
        Error.NotFound("Kafka.TopicNotFound", $"Kafka topic '{topic}' was not found");

    public static Error SchemaValidationFailed(string schemaName, string reason) =>
        Error.Validation("Kafka.SchemaValidationFailed", $"Schema validation failed for '{schemaName}': {reason}");
}
