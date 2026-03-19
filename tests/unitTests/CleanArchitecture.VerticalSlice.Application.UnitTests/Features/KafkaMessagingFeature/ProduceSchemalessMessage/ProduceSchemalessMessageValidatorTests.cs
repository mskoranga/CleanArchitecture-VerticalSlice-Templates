using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemalessMessage;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.KafkaMessagingFeature.ProduceSchemalessMessage;

public class ProduceSchemalessMessageValidatorTests
{
    private readonly ProduceSchemalessMessageValidator _validator;

    public ProduceSchemalessMessageValidatorTests()
    {
        _validator = new ProduceSchemalessMessageValidator();
    }

    [Fact]
    public void Validate_WhenTopicIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: "",
            Key: "test-key",
            Payload: new Dictionary<string, object> { { "test", "value" } });

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic)
            .WithErrorMessage("Topic is required");
    }

    [Theory]
    [InlineData("invalid topic")]
    [InlineData("topic!")]
    [InlineData("topic@name")]
    [InlineData("topic#123")]
    public void Validate_WhenTopicHasInvalidCharacters_ShouldHaveValidationError(string invalidTopic)
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: invalidTopic,
            Key: "test-key",
            Payload: new Dictionary<string, object> { { "test", "value" } });

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic)
            .WithErrorMessage("Topic must contain only alphanumeric characters, dots, underscores, and hyphens");
    }

    [Theory]
    [InlineData("valid-topic")]
    [InlineData("valid.topic")]
    [InlineData("valid_topic")]
    [InlineData("ValidTopic123")]
    [InlineData("topic-with.multiple_separators")]
    public void Validate_WhenTopicIsValid_ShouldNotHaveValidationError(string validTopic)
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: validTopic,
            Key: "test-key",
            Payload: new Dictionary<string, object> { { "test", "value" } });

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Topic);
    }

    [Fact]
    public void Validate_WhenTopicExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longTopic = new string('a', 250);
        var request = new ProduceSchemalessMessageRequest(
            Topic: longTopic,
            Key: "test-key",
            Payload: new Dictionary<string, object> { { "test", "value" } });

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic)
            .WithErrorMessage("Topic must not exceed 249 characters");
    }

    [Fact]
    public void Validate_WhenPayloadIsNull_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: "test-topic",
            Key: "test-key",
            Payload: null!);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload)
            .WithErrorMessage("Payload is required");
    }

    [Fact]
    public void Validate_WhenPayloadIsEmpty_ShouldHaveValidationError()
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: "test-topic",
            Key: "test-key",
            Payload: new Dictionary<string, object>());

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload)
            .WithErrorMessage("Payload must contain at least one key-value pair");
    }

    [Fact]
    public void Validate_WhenKeyExceedsMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longKey = new string('a', 501);
        var request = new ProduceSchemalessMessageRequest(
            Topic: "test-topic",
            Key: longKey,
            Payload: new Dictionary<string, object> { { "test", "value" } });

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Key)
            .WithErrorMessage("Key must not exceed 500 characters");
    }

    [Fact]
    public void Validate_WhenAllFieldsAreValid_ShouldNotHaveAnyValidationErrors()
    {
        // Arrange
        var request = new ProduceSchemalessMessageRequest(
            Topic: "test-topic",
            Key: "test-key",
            Payload: new Dictionary<string, object>
            {
                { "action", "login" },
                { "userId", "user-123" },
                { "timestamp", DateTime.UtcNow }
            });

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
