# Kafka Messaging Feature - Quick Start Guide

## What's Been Created

A complete Kafka messaging feature with:

### ✅ Produce Messages (2 ways)
1. **Schemaless Messages** - Flexible JSON payloads
2. **Schema-Based Messages** - Strongly-typed events with validation

### ✅ Consume Messages
- Read messages from any topic
- Configurable batch sizes
- Consumer group management

### ✅ Best Practices
- Clean Architecture & Vertical Slice Design
- FluentValidation for all inputs
- Comprehensive error handling with Result pattern
- Structured logging
- Async/await throughout
- CancellationToken support

## API Endpoints

### 1. Produce Schemaless Message
```http
POST /kafka/produce/schemaless
Content-Type: application/json

{
  "topic": "user-activities",
  "key": "user-123",
  "payload": {
    "action": "login",
    "userId": "user-123",
    "timestamp": "2024-03-10T10:00:00Z"
  }
}
```

### 2. Produce Order Event (Schema-Based)
```http
POST /kafka/produce/order-event?topic=orders
Content-Type: application/json

{
  "orderId": "ORD-12345",
  "customerId": "CUST-67890",
  "totalAmount": 299.99,
  "orderDate": "2024-03-10T10:00:00Z",
  "status": "Pending"
}
```

### 3. Produce User Event (Schema-Based)
```http
POST /kafka/produce/user-event?topic=user-registrations
Content-Type: application/json

{
  "userId": "USR-12345",
  "email": "user@example.com",
  "name": "John Doe",
  "registeredAt": "2024-03-10T10:00:00Z"
}
```

### 4. Consume Messages
```http
GET /kafka/consume/user-activities?consumerGroupId=my-group&maxMessages=10
```

## Configuration Required

Add to your `appsettings.json`:

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

## Files Created

```
src/Application/Features/KafkaMessagingFeature/
├── KafkaErrors.cs                                    # Error definitions
├── README.md                                         # Detailed documentation
├── Models/
│   └── KafkaMessage.cs                              # Message models
├── ProduceSchemalessMessage/
│   ├── ProduceSchemalessMessageHandler.cs          # Business logic
│   ├── ProduceSchemalessMessageEndpoint.cs         # API endpoint
│   └── ProduceSchemalessMessageValidator.cs        # Input validation
├── ProduceSchemaMessage/
│   ├── ProduceSchemaMessageHandler.cs              # Handlers for OrderEvent & UserEvent
│   ├── ProduceSchemaMessageEndpoint.cs             # API endpoints
│   └── ProduceSchemaMessageValidator.cs            # Schema & event validation
└── ConsumeMessages/
    ├── ConsumeMessagesHandler.cs                    # Consumer logic
    ├── ConsumeMessagesEndpoint.cs                   # API endpoint
    └── ConsumeMessagesValidator.cs                  # Input validation
```

## Testing Locally

### Prerequisites
1. Start Kafka locally:
```bash
docker run -d --name kafka -p 9092:9092 \
  apache/kafka:latest
```

### Test with cURL

1. **Produce a schemaless message:**
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "test-topic",
    "key": "test-key-1",
    "payload": {
      "message": "Hello Kafka!",
      "timestamp": "2024-03-10T10:00:00Z"
    }
  }'
```

2. **Produce an order event:**
```bash
curl -X POST "http://localhost:5000/kafka/produce/order-event?topic=orders" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-001",
    "customerId": "CUST-001",
    "totalAmount": 150.00,
    "orderDate": "2024-03-10T10:00:00Z",
    "status": "Pending"
  }'
```

3. **Consume messages:**
```bash
curl "http://localhost:5000/kafka/consume/test-topic?consumerGroupId=test-group&maxMessages=5"
```

## Key Features

### 🔐 Validation
- Topic name format validation
- Required field validation
- Business rule validation (e.g., amount > 0)
- Schema version format validation

### 📊 Logging
All operations include structured logging:
- Message IDs for traceability
- Topic and partition information
- Error details for debugging
- Performance metrics

### ⚡ Performance
- Async/await for non-blocking operations
- Configurable batch sizes
- Proper resource disposal
- Connection pooling via DI

### 🛡️ Error Handling
Comprehensive error types:
- `ProduceFailed`: When message production fails
- `ConsumeFailed`: When consumption fails
- `InvalidMessage`: For validation errors
- `TopicNotFound`: When topic doesn't exist
- `SchemaValidationFailed`: For schema mismatches

## Extending the Feature

### Add a New Event Type

1. Define in `Models/KafkaMessage.cs`:
```csharp
public sealed record PaymentProcessedEvent(
    string PaymentId,
    decimal Amount,
    string Status,
    DateTime ProcessedAt)
{
    public string Schema => "PaymentProcessedEvent.v1";
}
```

2. Create handler in `ProduceSchemaMessage/ProduceSchemaMessageHandler.cs`
3. Create endpoint in `ProduceSchemaMessage/ProduceSchemaMessageEndpoint.cs`
4. Create validator in `ProduceSchemaMessage/ProduceSchemaMessageValidator.cs`

The DI container will automatically discover and register your new handlers!

## Next Steps

1. ✅ Feature is ready to use
2. Configure Kafka connection in appsettings.json
3. Start producing and consuming messages
4. Monitor logs in Application Insights
5. Add unit tests for your specific use cases

## Support

For detailed documentation, see `README.md` in the feature folder.
For examples of adding new event types, refer to the existing `OrderCreatedEvent` and `UserRegisteredEvent`.
