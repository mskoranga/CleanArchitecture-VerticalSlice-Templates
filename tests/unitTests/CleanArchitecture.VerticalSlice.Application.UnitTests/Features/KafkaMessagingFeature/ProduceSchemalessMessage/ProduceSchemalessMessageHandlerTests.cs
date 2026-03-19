using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemalessMessage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.KafkaMessagingFeature.ProduceSchemalessMessage;

public class ProduceSchemalessMessageHandlerTests
{
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly Mock<ILogger<ProduceSchemalessMessageHandler>> _loggerMock;
    private readonly ProduceSchemalessMessageHandler _handler;

    public ProduceSchemalessMessageHandlerTests()
    {
        _kafkaProducerMock = new Mock<IKafkaProducer>();
        _loggerMock = new Mock<ILogger<ProduceSchemalessMessageHandler>>();
        _handler = new ProduceSchemalessMessageHandler(_kafkaProducerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WhenMessageProducedSuccessfully_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: "test-topic",
            Key: "test-key",
            Payload: new Dictionary<string, object>
            {
                { "action", "test" },
                { "value", 123 }
            });

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
        result.Value.Topic.Should().Be("test-topic");
        result.Value.MessageId.Should().NotBeNullOrEmpty();

        _kafkaProducerMock.Verify(
            x => x.ProduceAsync(
                "test-topic",
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WhenProducerThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: "test-topic",
            Key: "test-key",
            Payload: new Dictionary<string, object> { { "test", "value" } });

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Kafka connection failed"));

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Kafka.ProduceFailed");
        result.Error.Description.Should().Contain("test-topic");
        result.Error.Description.Should().Contain("Kafka connection failed");
    }

    [Fact]
    public async Task HandleAsync_WhenKeyIsNull_ShouldGenerateKey()
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: "test-topic",
            Key: null,
            Payload: new Dictionary<string, object> { { "test", "value" } });

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
        capturedKey.Should().NotBeNullOrEmpty();
    }
}
