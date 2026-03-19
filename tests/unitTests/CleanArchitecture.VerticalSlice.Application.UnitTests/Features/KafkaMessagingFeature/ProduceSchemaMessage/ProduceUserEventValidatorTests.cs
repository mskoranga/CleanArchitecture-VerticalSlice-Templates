using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemaMessage;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.KafkaMessagingFeature.ProduceSchemaMessage;

public class ProduceUserEventValidatorTests
{
    private readonly ProduceUserEventValidator _validator;

    public ProduceUserEventValidatorTests()
    {
        _validator = new ProduceUserEventValidator();
    }

    [Fact]
    public void Validate_WithValidUserEvent_ShouldNotHaveValidationErrors()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-12345",
            Email: "john.doe@example.com",
            Name: "John Doe",
            RegisteredAt: DateTime.UtcNow.AddMinutes(-5));

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations",
            Key: userEvent.UserId,
            Schema: userEvent.Schema,
            SchemaVersion: "1.0",
            Payload: userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyTopic_ShouldHaveValidationError()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", "Test", DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic)
            .WithErrorMessage("Topic is required");
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveValidationError()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent("", "test@example.com", "Test User", DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.UserId)
            .WithErrorMessage("UserId is required");
    }

    [Fact]
    public void Validate_WithEmptyEmail_ShouldHaveValidationError()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent("USR-1", "", "Test User", DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.Email)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_WithInvalidEmail_ShouldHaveValidationError(string invalidEmail)
    {
        // Arrange
        var userEvent = new UserRegisteredEvent("USR-1", invalidEmail, "Test User", DateTime.UtcNow.AddHours(-1));
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.Email)
            .WithErrorMessage("Email must be a valid email address");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user@example.com")]
    [InlineData("user+tag@example.co.uk")]
    [InlineData("user123@subdomain.example.com")]
    [InlineData("first.last@company-name.com")]
    public void Validate_WithValidEmail_ShouldNotHaveValidationError(string validEmail)
    {
        // Arrange
        var userEvent = new UserRegisteredEvent("USR-1", validEmail, "Test User", DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Payload.Email);
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveValidationError()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", "", DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.Name)
            .WithErrorMessage("Name is required");
    }

    [Fact]
    public void Validate_WithNameExceedingMaxLength_ShouldHaveValidationError()
    {
        // Arrange
        var longName = new string('A', 201);
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", longName, DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.Name)
            .WithErrorMessage("Name must not exceed 200 characters");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    [InlineData(200)]
    public void Validate_WithNameWithinMaxLength_ShouldNotHaveValidationError(int nameLength)
    {
        // Arrange
        var validName = new string('A', nameLength);
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", validName, DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Payload.Name);
    }

    [Fact]
    public void Validate_WithFutureRegistrationDate_ShouldHaveValidationError()
    {
        // Arrange
        var futureDate = DateTime.UtcNow.AddDays(1);
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", "Test User", futureDate);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Payload.RegisteredAt)
            .WithErrorMessage("RegisteredAt cannot be in the future");
    }

    [Fact]
    public void Validate_WithPastRegistrationDate_ShouldNotHaveValidationError()
    {
        // Arrange
        var pastDate = DateTime.UtcNow.AddHours(-1);
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", "Test User", pastDate);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Payload.RegisteredAt);
    }

    [Fact]
    public void Validate_WithCurrentRegistrationDate_ShouldNotHaveValidationError()
    {
        // Arrange - Use a slightly past date to avoid timing issues
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", "Test User", DateTime.UtcNow.AddSeconds(-1));
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Payload.RegisteredAt);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("1.0.0")]
    [InlineData("v1.0")]
    [InlineData("version-1")]
    public void Validate_WithInvalidSchemaVersion_ShouldHaveValidationError(string invalidVersion)
    {
        // Arrange
        var userEvent = new UserRegisteredEvent("USR-1", "test@example.com", "Test", DateTime.UtcNow);
        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", invalidVersion, userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SchemaVersion);
    }

    [Fact]
    public void Validate_WithCompleteValidData_ShouldNotHaveAnyErrors()
    {
        // Arrange - Complete user registration scenario
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-2024-03-10-ABCD1234",
            Email: "alice.smith@company.com",
            Name: "Alice Marie Smith",
            RegisteredAt: new DateTime(2024, 3, 10, 14, 30, 0, DateTimeKind.Utc));

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
            Topic: "user-registrations-production",
            Key: userEvent.UserId,
            Schema: "UserRegisteredEvent.v1",
            SchemaVersion: "1.0",
            Payload: userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithSpecialCharactersInName_ShouldNotHaveValidationError()
    {
        // Arrange
        var userEvent = new UserRegisteredEvent(
            UserId: "USR-SPECIAL",
            Email: "user@example.com",
            Name: "José María O'Brien-González",
            RegisteredAt: DateTime.UtcNow);

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("users", null, "Schema", "1.0", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Payload.Name);
    }

    [Fact]
    public void Validate_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        // Arrange - Invalid everything
        var userEvent = new UserRegisteredEvent(
            UserId: "",
            Email: "not-an-email",
            Name: "",
            RegisteredAt: DateTime.UtcNow.AddDays(1));

        var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>("", null, "", "invalid", userEvent);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Topic);
        result.ShouldHaveValidationErrorFor(x => x.Schema);
        result.ShouldHaveValidationErrorFor(x => x.SchemaVersion);
        result.ShouldHaveValidationErrorFor(x => x.Payload.UserId);
        result.ShouldHaveValidationErrorFor(x => x.Payload.Email);
        result.ShouldHaveValidationErrorFor(x => x.Payload.Name);
        result.ShouldHaveValidationErrorFor(x => x.Payload.RegisteredAt);
    }

    [Fact]
    public void Validate_WithRealisticTestData_ShouldValidateCorrectly()
    {
        // Arrange - Realistic test scenarios
        var testUsers = new[]
        {
            new UserRegisteredEvent("USR-001", "alice@example.com", "Alice Johnson", DateTime.UtcNow.AddMinutes(-10)),
            new UserRegisteredEvent("USR-002", "bob.smith@company.com", "Bob Smith", DateTime.UtcNow.AddHours(-1)),
            new UserRegisteredEvent("USR-003", "carol+test@email.co.uk", "Carol Williams", DateTime.UtcNow.AddDays(-1))
        };

        // Act & Assert
        foreach (var userEvent in testUsers)
        {
            var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
                "user-registrations",
                null,
                "UserRegisteredEvent.v1",
                "1.0",
                userEvent);

            var result = _validator.TestValidate(request);
            result.ShouldNotHaveAnyValidationErrors();
        }
    }
}
