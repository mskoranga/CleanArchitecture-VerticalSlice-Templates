using CleanArchitecture.VerticalSlice.Domain.Abstractions;
using CleanArchitecture.VerticalSlice.Domain.Abstractions.Errors;

namespace CleanArchitecture.VerticalSlice.Domain.UnitTests.Abstractions;

/// <summary>
/// Unit tests for the Result pattern implementation
/// Tests cover both success and failure scenarios
/// </summary>
public class ResultTests
{
    #region Success Tests

    [Fact]
    public void Success_ShouldCreateSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessResultWithValue()
    {
        // Arrange
        var expectedValue = "Test Value";

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(expectedValue);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Success_WithComplexObject_ShouldRetainObjectProperties()
    {
        // Arrange
        var testObject = new { Id = 1, Name = "Test" };

        // Act
        var result = Result.Success(testObject);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test");
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error description");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.Code.Should().Be("Test.Error");
        result.Error.Description.Should().Be("Test error description");
    }

    [Fact]
    public void Failure_WithValue_ShouldCreateFailureResultWithDefaultValue()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error description");

        // Act
        var result = Result.Failure<string>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_AccessingValue_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Test error description");
        var result = Result.Failure<string>(error);

        // Act
        var act = () => { var value = result.Value; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of a failure result can not be accessed.");
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_WithNonNullValue_ShouldReturnSuccessResult()
    {
        // Arrange
        var value = "Test Value";

        // Act
        var result = Result.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Create_WithNullValue_ShouldReturnFailureResult()
    {
        // Arrange
        string? value = null;

        // Act
        var result = Result.Create(value);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.Null);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    public void Create_WithValueType_ShouldReturnSuccessResult(int value)
    {
        // Act
        var result = Result.Create(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    #endregion

    #region Constructor Validation Tests

    // Note: Constructor validation tests are omitted as the Result constructor is protected
    // The validation logic is tested through the public factory methods above

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromValue_ShouldCreateSuccessResult()
    {
        // Arrange
        string value = "Test Value";

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ImplicitConversion_FromError_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Description");

        // Act
        Result<string> result = error;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void ImplicitConversion_FromNullValue_ShouldCreateFailureResult()
    {
        // Arrange
        string? value = null;

        // Act
        Result<string> result = value;

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Be(Error.Null);
    }

    #endregion
}
