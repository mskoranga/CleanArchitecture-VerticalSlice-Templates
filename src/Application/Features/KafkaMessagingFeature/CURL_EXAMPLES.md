# Kafka Messaging Feature - cURL Examples

This file contains ready-to-use cURL commands for testing all Kafka messaging endpoints.

## Prerequisites

1. Application running on `http://localhost:5000`
2. Kafka broker running on `localhost:9092`
3. Topics created (or auto-create enabled)

---

## 1. Produce Schemaless Message

### Basic Example
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "user-activities",
    "key": "user-123",
    "payload": {
      "action": "login",
      "userId": "user-123",
      "timestamp": "2024-03-10T10:00:00Z",
      "metadata": {
        "ip": "192.168.1.1",
        "device": "mobile"
      }
    }
  }'
```

### Page View Event
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "analytics",
    "key": "session-789",
    "payload": {
      "event": "page_view",
      "page": "/products",
      "userId": "user-456",
      "duration": 45.2,
      "referrer": "https://google.com"
    }
  }'
```

### Shopping Cart Event
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "cart-events",
    "payload": {
      "event": "item_added",
      "cartId": "cart-123",
      "productId": "prod-789",
      "quantity": 2,
      "price": 49.99
    }
  }'
```

---

## 2. Produce Order Event (Schema-Based)

### Pending Order
```bash
curl -X POST "http://localhost:5000/kafka/produce/order-event?topic=orders" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-12345",
    "customerId": "CUST-67890",
    "totalAmount": 299.99,
    "orderDate": "2024-03-10T10:00:00Z",
    "status": "Pending"
  }'
```

### Completed Order
```bash
curl -X POST "http://localhost:5000/kafka/produce/order-event?topic=orders" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-67890",
    "customerId": "CUST-11111",
    "totalAmount": 150.00,
    "orderDate": "2024-03-10T09:30:00Z",
    "status": "Completed"
  }'
```

### Cancelled Order
```bash
curl -X POST "http://localhost:5000/kafka/produce/order-event?topic=orders" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-99999",
    "customerId": "CUST-22222",
    "totalAmount": 89.99,
    "orderDate": "2024-03-10T11:15:00Z",
    "status": "Cancelled"
  }'
```

---

## 3. Produce User Registration Event (Schema-Based)

### New User Registration
```bash
curl -X POST "http://localhost:5000/kafka/produce/user-event?topic=user-registrations" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "USR-12345",
    "email": "john.doe@example.com",
    "name": "John Doe",
    "registeredAt": "2024-03-10T10:00:00Z"
  }'
```

### Multiple Users (Loop)
```bash
for i in {1..5}; do
  curl -X POST "http://localhost:5000/kafka/produce/user-event?topic=user-registrations" \
    -H "Content-Type: application/json" \
    -d "{
      \"userId\": \"USR-$i\",
      \"email\": \"user$i@example.com\",
      \"name\": \"User $i\",
      \"registeredAt\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\"
    }"
  sleep 1
done
```

---

## 4. Consume Messages

### Consume from Topic
```bash
curl -X GET "http://localhost:5000/kafka/consume/user-activities?consumerGroupId=my-consumer-group&maxMessages=10"
```

### Consume Orders
```bash
curl -X GET "http://localhost:5000/kafka/consume/orders?consumerGroupId=order-processor&maxMessages=50"
```

### Consume User Registrations
```bash
curl -X GET "http://localhost:5000/kafka/consume/user-registrations?consumerGroupId=user-processor&maxMessages=20"
```

---

## 5. Validation Error Examples

### Empty Topic (Should Fail)
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "",
    "key": "test-key",
    "payload": {
      "test": "value"
    }
  }'
```

### Invalid Topic Name (Should Fail)
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "invalid topic!",
    "key": "test-key",
    "payload": {
      "test": "value"
    }
  }'
```

### Empty Payload (Should Fail)
```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "test-topic",
    "key": "test-key",
    "payload": {}
  }'
```

### Invalid Order Status (Should Fail)
```bash
curl -X POST "http://localhost:5000/kafka/produce/order-event?topic=orders" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-12345",
    "customerId": "CUST-67890",
    "totalAmount": 299.99,
    "orderDate": "2024-03-10T10:00:00Z",
    "status": "InvalidStatus"
  }'
```

### Negative Amount (Should Fail)
```bash
curl -X POST "http://localhost:5000/kafka/produce/order-event?topic=orders" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": "ORD-12345",
    "customerId": "CUST-67890",
    "totalAmount": -50.00,
    "orderDate": "2024-03-10T10:00:00Z",
    "status": "Pending"
  }'
```

### Invalid Email (Should Fail)
```bash
curl -X POST "http://localhost:5000/kafka/produce/user-event?topic=user-registrations" \
  -H "Content-Type: application/json" \
  -d '{
    "userId": "USR-12345",
    "email": "not-an-email",
    "name": "John Doe",
    "registeredAt": "2024-03-10T10:00:00Z"
  }'
```

---

## 6. Performance Testing

### Load Test - 100 Messages
```bash
for i in {1..100}; do
  curl -X POST http://localhost:5000/kafka/produce/schemaless \
    -H "Content-Type: application/json" \
    -d "{
      \"topic\": \"load-test\",
      \"key\": \"key-$i\",
      \"payload\": {
        \"index\": $i,
        \"timestamp\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",
        \"data\": \"test data $i\"
      }
    }" &
done
wait
echo "Load test completed"
```

### Batch Produce Orders
```bash
for i in {1..20}; do
  curl -X POST "http://localhost:5000/kafka/produce/order-event?topic=orders" \
    -H "Content-Type: application/json" \
    -d "{
      \"orderId\": \"ORD-BATCH-$i\",
      \"customerId\": \"CUST-$((1000 + i))\",
      \"totalAmount\": $((50 + i * 10)).99,
      \"orderDate\": \"$(date -u +%Y-%m-%dT%H:%M:%SZ)\",
      \"status\": \"Pending\"
    }" > /dev/null 2>&1
  echo "Produced order $i"
done
```

---

## 7. Pretty Print Responses (with jq)

If you have `jq` installed, you can format responses:

```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "test-topic",
    "key": "test-key",
    "payload": {
      "action": "test",
      "value": 123
    }
  }' | jq '.'
```

```bash
curl -X GET "http://localhost:5000/kafka/consume/orders?consumerGroupId=test&maxMessages=5" | jq '.'
```

---

## 8. Save Response to File

```bash
curl -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "test-topic",
    "payload": {
      "test": "data"
    }
  }' -o response.json
```

---

## 9. Verbose Output (Debug)

Add `-v` flag for detailed request/response information:

```bash
curl -v -X POST http://localhost:5000/kafka/produce/schemaless \
  -H "Content-Type: application/json" \
  -d '{
    "topic": "test-topic",
    "payload": {
      "test": "data"
    }
  }'
```

---

## 10. Using Environment Variables

```bash
export API_BASE_URL="http://localhost:5000"
export KAFKA_TOPIC="my-topic"

curl -X POST "$API_BASE_URL/kafka/produce/schemaless" \
  -H "Content-Type: application/json" \
  -d "{
    \"topic\": \"$KAFKA_TOPIC\",
    \"payload\": {
      \"test\": \"data\"
    }
  }"
```

---

## Tips

1. **Replace timestamps**: Use `$(date -u +%Y-%m-%dT%H:%M:%SZ)` for current timestamp
2. **Generate GUIDs**: Use `$(uuidgen)` for unique IDs (on Linux/Mac)
3. **Loop testing**: Use bash loops for batch testing
4. **Save responses**: Use `-o filename.json` to save responses
5. **Silent mode**: Use `-s` flag to suppress progress meters
6. **Pretty print**: Pipe to `jq` for formatted JSON output

---

## Expected Response Formats

### Success Response (Produce)
```json
{
  "messageId": "01939c8b-1234-7890-abcd-ef0123456789",
  "topic": "user-activities",
  "producedAt": "2024-03-10T10:00:00.123Z"
}
```

### Success Response (Consume)
```json
{
  "topic": "user-activities",
  "messageCount": 5,
  "messages": [
    {
      "messageId": "01939c8b-1234-7890-abcd-ef0123456789",
      "key": "user-123",
      "payload": { ... },
      "consumedAt": "2024-03-10T10:00:05.123Z"
    }
  ]
}
```

### Error Response
```json
{
  "code": "Kafka.ProduceFailed",
  "description": "Failed to produce message to topic 'test-topic': Connection refused",
  "type": "Failure"
}
```
