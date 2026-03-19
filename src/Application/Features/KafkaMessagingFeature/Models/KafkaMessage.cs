namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;

/// <summary>
/// Base message without schema - flexible key-value payload
/// </summary>
public sealed record SchemalessMessage(
    string Key,
    Dictionary<string, object> Payload,
    DateTime? Timestamp = null);

/// <summary>
/// Strongly-typed message with schema validation
/// </summary>
public sealed record OrderCreatedEvent(
    string OrderId,
    string CustomerId,
    decimal TotalAmount,
    DateTime OrderDate,
    string Status)
{
    public string Schema => "OrderCreatedEvent.v1";
}

/// <summary>
/// Strongly-typed message with schema validation
/// </summary>
public sealed record UserRegisteredEvent(
    string UserId,
    string Email,
    string Name,
    DateTime RegisteredAt)
{
    public string Schema => "UserRegisteredEvent.v1";
}

/// <summary>
/// Generic schema-based message wrapper
/// </summary>
/// <typeparam name="T">The payload type</typeparam>
public sealed record SchemaMessage<T>(
    string MessageId,
    string Schema,
    string SchemaVersion,
    T Payload,
    DateTime Timestamp,
    Dictionary<string, string>? Metadata = null) where T : class;
