using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemaMessage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.KafkaMessagingFeature.ProduceSchemaMessage;

public class ProduceOrderEventHandlerTests
{
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly Mock<ILogger<ProduceOrderEventHandler>> _loggerMock;
    private readonly ProduceOrderEventHandler _handler;

    public ProduceOrderEventHandlerTests()
    {
        _kafkaProducerMock = new Mock<IKafkaProducer>();
        _loggerMock = new Mock<ILogger<ProduceOrderEventHandler>>();
        _handler = new ProduceOrderEventHandler(_kafkaProducerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidOrderEvent_ShouldProduceMessageSuccessfully()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-12345",
            CustomerId: "CUST-67890",
            TotalAmount: 299.99m,
            OrderDate: new DateTime(2024, 3, 10, 10, 0, 0, DateTimeKind.Utc),
            Status: "Pending");

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders",
            Key: orderEvent.OrderId,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent,
            Metadata: null);

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SchemaMessage<OrderCreatedEvent>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Topic.Should().Be("orders");
        result.Value.Schema.Should().Be("OrderCreatedEvent.v1");
        result.Value.MessageId.Should().NotBeNullOrEmpty();
        result.Value.ProducedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _kafkaProducerMock.Verify(
            x => x.ProduceAsync(
                "orders",
                "ORD-12345",
                It.Is<SchemaMessage<OrderCreatedEvent>>(m => 
                    m.Schema == "OrderCreatedEvent.v1" &&
                    m.SchemaVersion == "1.0" &&
                    m.Payload.OrderId == "ORD-12345" &&
                    m.Payload.CustomerId == "CUST-67890" &&
                    m.Payload.TotalAmount == 299.99m &&
                    m.Payload.Status == "Pending"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCompletedOrder_ShouldProduceMessageSuccessfully()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-99999",
            CustomerId: "CUST-11111",
            TotalAmount: 1499.99m,
            OrderDate: new DateTime(2024, 3, 10, 9, 30, 0, DateTimeKind.Utc),
            Status: "Completed");

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders-completed",
            Key: orderEvent.OrderId,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent);

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Topic.Should().Be("orders-completed");
        result.Value.Schema.Should().Be("OrderCreatedEvent.v1");
    }

    [Fact]
    public async Task HandleAsync_WithCustomMetadata_ShouldIncludeMetadataInMessage()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-55555",
            CustomerId: "CUST-22222",
            TotalAmount: 89.99m,
            OrderDate: DateTime.UtcNow.AddHours(-1),
            Status: "Processing");

        var metadata = new Dictionary<string, string>
        {
            { "CorrelationId", "corr-123" },
            { "UserId", "user-456" },
            { "Source", "WebApp" }
        };

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders",
            Key: orderEvent.OrderId,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent,
            Metadata: metadata);

        SchemaMessage<OrderCreatedEvent>? capturedMessage = null;
        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SchemaMessage<OrderCreatedEvent>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, SchemaMessage<OrderCreatedEvent>, CancellationToken>((_, _, msg, _) => capturedMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedMessage.Should().NotBeNull();
        capturedMessage!.Metadata.Should().ContainKeys("CorrelationId", "UserId", "Source");
        capturedMessage.Metadata!["CorrelationId"].Should().Be("corr-123");
        capturedMessage.Metadata["UserId"].Should().Be("user-456");
        capturedMessage.Metadata["Source"].Should().Be("WebApp");
    }

    [Fact]
    public async Task HandleAsync_WithNullKey_ShouldUseOrderIdAsKey()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-77777",
            CustomerId: "CUST-33333",
            TotalAmount: 199.99m,
            OrderDate: DateTime.UtcNow,
            Status: "Pending");

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders",
            Key: null,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent);

        string? capturedKey = null;
        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, object, CancellationToken>((_, key, _, _) => capturedKey = key)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedKey.Should().Be("ORD-77777");
    }

    [Fact]
    public async Task HandleAsync_WhenProducerThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-ERROR",
            CustomerId: "CUST-ERROR",
            TotalAmount: 50.00m,
            OrderDate: DateTime.UtcNow,
            Status: "Pending");

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders",
            Key: orderEvent.OrderId,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent);

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Kafka broker not available"));

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Kafka.ProduceFailed");
        result.Error.Description.Should().Contain("orders");
        result.Error.Description.Should().Contain("Kafka broker not available");
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Processing")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public async Task HandleAsync_WithDifferentStatuses_ShouldProduceSuccessfully(string status)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: $"ORD-{status.ToUpper()}",
            CustomerId: "CUST-12345",
            TotalAmount: 100.00m,
            OrderDate: DateTime.UtcNow,
            Status: status);

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders",
            Key: orderEvent.OrderId,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent);

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_WithHighValueOrder_ShouldProduceSuccessfully()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-HIGH-VALUE",
            CustomerId: "CUST-VIP-001",
            TotalAmount: 99999.99m,
            OrderDate: DateTime.UtcNow,
            Status: "Pending");

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders-high-value",
            Key: orderEvent.OrderId,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent);

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        _kafkaProducerMock.Verify(
            x => x.ProduceAsync(
                "orders-high-value",
                It.IsAny<string>(),
                It.Is<SchemaMessage<OrderCreatedEvent>>(m => m.Payload.TotalAmount == 99999.99m),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
