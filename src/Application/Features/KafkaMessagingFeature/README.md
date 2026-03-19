# Kafka Messaging Feature

This feature provides comprehensive Kafka messaging capabilities with support for both schema-based and schema-less messages, following clean architecture and vertical slice design principles.

## Features

### 1. **Produce Schemaless Messages**
Send flexible key-value messages to Kafka without schema validation.

**Endpoint:** `POST /kafka/produce/schemaless`

**Request:**
```json
{
  "topic": "user-activities",
  "key": "user-123",
  "payload": {
    "action": "login",
    "timestamp": "2024-03-10T10:00:00Z",
    "metadata": {
      "ip": "192.168.1.1",
      "device": "mobile"
    }
  }
}
```

**Response:**
```json
{
  "messageId": "01939c8b-1234-7890-abcd-ef0123456789",
  "topic": "user-activities",
  "producedAt": "2024-03-10T10:00:00.123Z"
}
```

### 2. **Produce Schema-Based Messages**

#### Order Events
Send strongly-typed order events with schema validation.

**Endpoint:** `POST /kafka/produce/order-event?topic=orders`

**Request:**
```json
{
  "orderId": "ORD-12345",
  "customerId": "CUST-67890",
  "totalAmount": 299.99,
  "orderDate": "2024-03-10T10:00:00Z",
  "status": "Pending"
}
```

#### User Events
Send strongly-typed user registration events with schema validation.

**Endpoint:** `POST /kafka/produce/user-event?topic=user-registrations`

**Request:**
```json
{
  "userId": "USR-12345",
  "email": "user@example.com",
  "name": "John Doe",
  "registeredAt": "2024-03-10T10:00:00Z"
}
```

**Response (both endpoints):**
```json
{
  "messageId": "01939c8b-1234-7890-abcd-ef0123456789",
  "topic": "orders",
  "schema": "OrderCreatedEvent.v1",
  "producedAt": "2024-03-10T10:00:00.123Z"
}
```

### 3. **Consume Messages**
Consume messages from any Kafka topic.

**Endpoint:** `GET /kafka/consume/{topic}?consumerGroupId=my-group&maxMessages=10`

**Response:**
```json
{
  "topic": "user-activities",
  "messageCount": 5,
  "messages": [
    {
      "messageId": "01939c8b-1234-7890-abcd-ef0123456789",
      "key": "user-123",
      "payload": {
        "action": "login",
        "timestamp": "2024-03-10T10:00:00Z"
      },
      "consumedAt": "2024-03-10T10:00:05.123Z",
      "schema": null
    }
  ]
}
```

## Architecture

### Vertical Slice Organization

```
Features/
└── KafkaMessagingFeature/
    ├── KafkaErrors.cs                          # Centralized error definitions
    ├── Models/
    │   └── KafkaMessage.cs                     # Message models
    ├── ProduceSchemalessMessage/
    │   ├── ProduceSchemalessMessageHandler.cs
    │   ├── ProduceSchemalessMessageEndpoint.cs
    │   └── ProduceSchemalessMessageValidator.cs
    ├── ProduceSchemaMessage/
    │   ├── ProduceSchemaMessageHandler.cs
    │   ├── ProduceSchemaMessageEndpoint.cs
    │   └── ProduceSchemaMessageValidator.cs
    └── ConsumeMessages/
        ├── ConsumeMessagesHandler.cs
        ├── ConsumeMessagesEndpoint.cs
        └── ConsumeMessagesValidator.cs
```

### Message Types

#### 1. SchemalessMessage
Flexible message without predefined structure:
- **Key**: Message partition key
- **Payload**: Dictionary of key-value pairs
- **Timestamp**: Optional timestamp

#### 2. Schema-Based Messages
Strongly-typed messages with validation:
- **OrderCreatedEvent**: Order creation events
- **UserRegisteredEvent**: User registration events
- **SchemaMessage<T>**: Generic wrapper with metadata

## Best Practices Implemented

### 1. **Message Keys**
- Auto-generated UUID v7 if not provided
- Ensures proper partitioning
- Maintains ordering within partitions

### 2. **Error Handling**
- Comprehensive error types (ProduceFailed, ConsumeFailed, InvalidMessage)
- Structured error responses with Result pattern
- Detailed logging for troubleshooting

### 3. **Validation**
- FluentValidation for all requests
- Topic name format validation (alphanumeric, dots, underscores, hyphens)
- Schema version validation (major.minor format)
- Business rule validation for events

### 4. **Metadata Enrichment**
- Automatic timestamp addition
- Source tracking
- Environment identification
- Custom metadata support

### 5. **Logging**
- Structured logging with context
- Message IDs for traceability
- Error details for debugging

### 6. **Scalability**
- Async/await throughout
- CancellationToken support
- Configurable consumer batch sizes
- Independent producer/consumer lifecycles

## Configuration

Add to `appsettings.json`:

```json
{
  "Kafka": {
    "BootstrapServers": "localhost:9092",
    "Producer": {
      "ClientId": "my-service-producer",
      "Acks": "All"
    },
    "Consumer": {
      "GroupId": "my-service-consumer-group",
      "AutoOffsetReset": "Earliest"
    }
  }
}
```

### Configuration Options

#### Producer Settings
- **ClientId**: Identifies the producer client
- **Acks**: 
  - `All` (default): All replicas must acknowledge
  - `Leader`: Only leader acknowledges
  - `None`: Fire and forget

#### Consumer Settings
- **GroupId**: Consumer group for load balancing
- **AutoOffsetReset**:
  - `Earliest`: Start from beginning
  - `Latest`: Start from newest messages

## Usage Examples

### Example 1: Sending Order Events
```csharp
// The endpoint handles this automatically
POST /kafka/produce/order-event?topic=orders
{
  "orderId": "ORD-12345",
  "customerId": "CUST-67890",
  "totalAmount": 299.99,
  "orderDate": "2024-03-10T10:00:00Z",
  "status": "Pending"
}
```

### Example 2: Sending Custom Events
```csharp
POST /kafka/produce/schemaless
{
  "topic": "analytics",
  "key": "page-view-123",
  "payload": {
    "event": "page_view",
    "userId": "user-456",
    "page": "/products",
    "duration": 45.2
  }
}
```

### Example 3: Consuming Messages
```csharp
GET /kafka/consume/orders?consumerGroupId=order-processor&maxMessages=50
```

## Extending the Feature

### Adding New Schema-Based Events

1. **Define the event model** in `Models/KafkaMessage.cs`:
```csharp
public sealed record ProductCreatedEvent(
    string ProductId,
    string Name,
    decimal Price,
    DateTime CreatedAt)
{
    public string Schema => "ProductCreatedEvent.v1";
}
```

2. **Create handler** in `ProduceSchemaMessage/ProduceSchemaMessageHandler.cs`:
```csharp
public sealed class ProduceProductEventHandler(
    IKafkaProducer _kafkaProducer,
    ILogger<ProduceProductEventHandler> _logger)
    : IHandler<ProduceSchemaMessageRequest<ProductCreatedEvent>, Result<ProduceSchemaMessageResponse>>
{
    // Implementation similar to existing handlers
}
```

3. **Create endpoint** in `ProduceSchemaMessage/ProduceSchemaMessageEndpoint.cs`:
```csharp
internal sealed class ProduceProductEventEndpoint : IApiEndpoint
{
    // Implementation similar to existing endpoints
}
```

4. **Create validator** in `ProduceSchemaMessage/ProduceSchemaMessageValidator.cs`:
```csharp
public sealed class ProductCreatedEventValidator : AbstractValidator<ProductCreatedEvent>
{
    // Validation rules
}
```

## Testing

The feature is designed for easy testing:

1. **Unit Tests**: Test handlers in isolation
2. **Integration Tests**: Test with actual Kafka (Testcontainers)
3. **Validation Tests**: Test FluentValidation rules

## Dependencies

- **Confluent.Kafka**: Kafka client library
- **FluentValidation**: Request validation
- **Microsoft.Extensions.Logging**: Structured logging
- **System.Text.Json**: JSON serialization

## Performance Considerations

1. **Batching**: Consider implementing batching for high-throughput scenarios
2. **Compression**: Enable compression in Kafka configuration for large messages
3. **Partitioning**: Use meaningful keys for optimal distribution
4. **Connection Pooling**: Producers/consumers are registered as singletons/transients appropriately

## Security

1. **Authentication**: Add SASL/SSL configuration in KafkaOptions
2. **Authorization**: Implement ACLs at Kafka broker level
3. **Encryption**: Use SSL for in-transit encryption
4. **Validation**: All inputs are validated before processing

## Monitoring

Recommended metrics to track:
- Message production rate
- Message consumption rate
- Error rates
- Lag per consumer group
- Message size distribution

Use Application Insights or Prometheus for metrics collection.
