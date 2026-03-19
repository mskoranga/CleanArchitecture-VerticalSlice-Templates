using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemaMessage;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.KafkaMessagingFeature.ProduceSchemaMessage;

public class ProduceOrderEventValidatorTests
{
    private readonly ProduceOrderEventValidator _validator;

    public ProduceOrderEventValidatorTests()
    {
        _validator = new ProduceOrderEventValidator();
    }

    [Fact]
    public void Validate_WithValidOrderEvent_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-12345",
            CustomerId: "CUST-67890",
            TotalAmount: 299.99m,
            OrderDate: DateTime.UtcNow.AddHours(-1),
            Status: "Pending");

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders",
            Key: orderEvent.OrderId,
            Schema: orderEvent.Schema,
            SchemaVersion: "1.0",
            Payload: orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTopic_ShouldHaveValidationError()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("", null, "Schema", "1.0", orderEvent);

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
    public void Validate_WithInvalidTopicCharacters_ShouldHaveValidationError(string invalidTopic)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(invalidTopic, null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic);
    }

    [Fact]
    public void Validate_WithEmptySchema_ShouldHaveValidationError()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Schema)
            .WithErrorMessage("Schema is required");
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1.0.0")]
    [InlineData("v1.0")]
    [InlineData("1-0")]
    public void Validate_WithInvalidSchemaVersion_ShouldHaveValidationError(string invalidVersion)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", invalidVersion, orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaVersion)
            .WithErrorMessage("Schema version must be in format 'major.minor' (e.g., '1.0')");
    }

    [Theory]
    [InlineData("1.0")]
    [InlineData("2.5")]
    [InlineData("10.15")]
    public void Validate_WithValidSchemaVersion_ShouldNotHaveValidationError(string validVersion)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", validVersion, orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SchemaVersion);
    }

    [Fact]
    public void Validate_WithEmptyOrderId_ShouldHaveValidationError()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("", "CUST-1", 100m, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.OrderId)
            .WithErrorMessage("OrderId is required");
    }

    [Fact]
    public void Validate_WithEmptyCustomerId_ShouldHaveValidationError()
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "", 100m, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.CustomerId)
            .WithErrorMessage("CustomerId is required");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100.50)]
    public void Validate_WithInvalidTotalAmount_ShouldHaveValidationError(decimal amount)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", amount, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.TotalAmount)
            .WithErrorMessage("TotalAmount must be greater than 0");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(999.99)]
    [InlineData(99999.99)]
    public void Validate_WithValidTotalAmount_ShouldNotHaveValidationError(decimal amount)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", amount, DateTime.UtcNow, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Payload.TotalAmount);
    }

    [Fact]
    public void Validate_WithFutureOrderDate_ShouldHaveValidationError()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, futureDate, "Pending");
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.OrderDate)
            .WithErrorMessage("OrderDate cannot be in the future");
    }

    [Theory]
    [InlineData("Pending")]
    [InlineData("Processing")]
    [InlineData("Completed")]
    [InlineData("Cancelled")]
    public void Validate_WithValidStatus_ShouldNotHaveValidationError(string status)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, DateTime.UtcNow, status);
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Payload.Status);
    }

    [Theory]
    [InlineData("")]
    [InlineData("InvalidStatus")]
    [InlineData("PENDING")]
    [InlineData("pending")]
    [InlineData("InProgress")]
    public void Validate_WithInvalidStatus_ShouldHaveValidationError(string status)
    {
        // Arrange
        var orderEvent = new OrderCreatedEvent("ORD-1", "CUST-1", 100m, DateTime.UtcNow, status);
        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>("orders", null, "Schema", "1.0", orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.Status);
    }

    [Fact]
    public void Validate_WithCompleteValidData_ShouldNotHaveAnyErrors()
    {
        // Arrange - Complete order scenario
        var orderEvent = new OrderCreatedEvent(
            OrderId: "ORD-2024-03-10-12345",
            CustomerId: "CUST-VIP-67890",
            TotalAmount: 1299.99m,
            OrderDate: new DateTime(2024, 3, 10, 10, 30, 0, DateTimeKind.Utc),
            Status: "Processing");

        var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
            Topic: "orders-production",
            Key: orderEvent.OrderId,
            Schema: "OrderCreatedEvent.v1",
            SchemaVersion: "1.0",
            Payload: orderEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }
}
