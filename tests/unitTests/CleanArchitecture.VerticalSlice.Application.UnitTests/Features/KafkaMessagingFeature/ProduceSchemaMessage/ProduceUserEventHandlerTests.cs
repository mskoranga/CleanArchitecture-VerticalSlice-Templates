using CleanArchitecture.VerticalSlice.Application.Abstractions.Messaging;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemaMessage;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.KafkaMessagingFeature.ProduceSchemaMessage;

public class ProduceUserEventHandlerTests
{
    private readonly Mock<IKafkaProducer> _kafkaProducerMock;
    private readonly Mock<ILogger<ProduceUserEventHandler>> _loggerMock;
    private readonly ProduceUserEventHandler _handler;

    public ProduceUserEventHandlerTests()
    {
        _kafkaProducerMock = new Mock<IKafkaProducer>();
        _loggerMock = new Mock<ILogger<ProduceUserEventHandler>>();
        _handler = new ProduceUserEventHandler(_kafkaProducerMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task HandleAsync_WithValidUserEvent_ShouldProduceMessageSuccessfully()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-12345",
            Email: "john.doe@example.com",
            Name: "John Doe",
            RegisteredAt: new DateTime(2024, 3, 10, 10, 0, 0, DateTimeKind.Utc));

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: userEvent.UserId,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent,
            Metadata: null);

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SchemaMessage<UserRegisteredEvent>>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Topic.Should().Be("user-registrations");
        result.Value.Schema.Should().Be("UserRegisteredEvent.v1");
        result.Value.MessageId.Should().NotBeNullOrEmpty();
        result.Value.ProducedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _kafkaProducerMock.Verify(
            x => x.ProduceAsync(
                "user-registrations",
                "USR-12345",
                It.Is<SchemaMessage<UserRegisteredEvent>>(m =>
                    m.Schema == "UserRegisteredEvent.v1" &&
                    m.SchemaVersion == "1.0" &&
                    m.Payload.UserId == "USR-12345" &&
                    m.Payload.Email == "john.doe@example.com" &&
                    m.Payload.Name == "John Doe"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithMultipleUsers_ShouldProduceAllSuccessfully()
    {
        // Arrange
        var testUsers = new[]
        {
            new UserRegisteredEvent("USR-001", "alice@example.com", "Alice Smith", DateTime.UtcNow),
            new UserRegisteredEvent("USR-002", "bob@example.com", "Bob Johnson", DateTime.UtcNow),
            new UserRegisteredEvent("USR-003", "carol@example.com", "Carol Williams", DateTime.UtcNow)
        };

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act & Assert
        foreach (var userEvent in testUsers)
        {
            var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
                Topic: "user-registrations",
                Key: userEvent.UserId,
                Schema: userEvent.Schema,
                SchemaVersion: "1.0",
                Payload: userEvent);

            var result = await _handler.HandleAsync(request, CancellationToken.None);
            result.IsSuccess.Should().BeTrue();
        }

        _kafkaProducerMock.Verify(
            x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task HandleAsync_WithCustomMetadata_ShouldIncludeMetadataInMessage()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-99999",
            Email: "jane.doe@example.com",
            Name: "Jane Doe",
            RegisteredAt: DateTime.UtcNow);

        var metadata = new Dictionary<string, string>
        {
            { "RegistrationSource", "Mobile" },
            { "ReferralCode", "REF-123" },
            { "Campaign", "Spring2024" }
        };

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: userEvent.UserId,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent,
            Metadata: metadata);

        SchemaMessage<UserRegisteredEvent>? capturedMessage = null;
        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<SchemaMessage<UserRegisteredEvent>>(),
                It.IsAny<CancellationToken>()))
            .Callback<string, string, SchemaMessage<UserRegisteredEvent>, CancellationToken>((_, _, msg, _) => capturedMessage = msg)
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        capturedMessage.Should().NotBeNull();
        capturedMessage!.Metadata.Should().ContainKeys("RegistrationSource", "ReferralCode", "Campaign");
        capturedMessage.Metadata!["RegistrationSource"].Should().Be("Mobile");
        capturedMessage.Metadata["ReferralCode"].Should().Be("REF-123");
        capturedMessage.Metadata["Campaign"].Should().Be("Spring2024");
    }

    [Fact]
    public async Task HandleAsync_WithNullKey_ShouldUseUserIdAsKey()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-77777",
            Email: "test@example.com",
            Name: "Test User",
            RegisteredAt: DateTime.UtcNow);

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: null,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent);

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
        capturedKey.Should().Be("USR-77777");
    }

    [Fact]
    public async Task HandleAsync_WhenProducerThrowsException_ShouldReturnFailureResult()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-ERROR",
            Email: "error@example.com",
            Name: "Error User",
            RegisteredAt: DateTime.UtcNow);

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: userEvent.UserId,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent);

        _kafkaProducerMock
            .Setup(x => x.ProduceAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Network timeout"));

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Code.Should().Be("Kafka.ProduceFailed");
        result.Error.Description.Should().Contain("user-registrations");
        result.Error.Description.Should().Contain("Network timeout");
    }

    [Theory]
    [InlineData("user@example.com", "John Smith")]
    [InlineData("alice.jones@company.com", "Alice Jones")]
    [InlineData("bob123@email.co.uk", "Bob Williams")]
    [InlineData("test+user@domain.com", "Test User")]
    public async Task HandleAsync_WithVariousEmailFormats_ShouldProduceSuccessfully(string email, string name)
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: $"USR-{Guid.NewGuid().ToString()[..8]}",
            Email: email,
            Name: name,
            RegisteredAt: DateTime.UtcNow);

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: userEvent.UserId,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent);

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
    public async Task HandleAsync_WithLongUserName_ShouldProduceSuccessfully()
    {
        // Arrange
        var longName = "Dr. Elizabeth Alexandra Mary Windsor-Mountbatten Von Habsburg-Lorraine III";
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-LONG-NAME",
            Email: "elizabeth@royal.uk",
            Name: longName,
            RegisteredAt: DateTime.UtcNow);

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: userEvent.UserId,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent);

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
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.Is<SchemaMessage<UserRegisteredEvent>>(m => m.Payload.Name == longName),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithSpecialCharactersInName_ShouldProduceSuccessfully()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-SPECIAL",
            Email: "user@example.com",
            Name: "José María O'Brien-González",
            RegisteredAt: DateTime.UtcNow);

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: userEvent.UserId,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent);

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
}
