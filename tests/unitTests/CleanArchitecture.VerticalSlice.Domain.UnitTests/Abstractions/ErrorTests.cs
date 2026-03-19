using CleanArchitecture.VerticalSlice.Domain.Abstractions;
using CleanArchitecture.VerticalSlice.Domain.Abstractions.Errors;

namespace CleanArchitecture.VerticalSlice.Domain.UnitTests.Abstractions;

/// <summary>
/// Unit tests for Error record and ErrorType enum
/// Tests all error factory methods and properties
/// </summary>
public class ErrorTests
{
    #region Static Error Tests

    [Fact]
    public void None_ShouldHaveEmptyCode()
    {
        // Act & Assert
        Error.None.Code.Should().BeEmpty();
        Error.None.Description.Should().BeNull();
        Error.None.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Null_ShouldHavePredefinedValues()
    {
        // Act & Assert
        Error.Null.Code.Should().Be("Error.NullValue");
        Error.Null.Description.Should().Be("The specified result value is null.");
        Error.Null.Type.Should().Be(ErrorType.Failure);
    }

    #endregion

    #region Factory Method Tests

    [Fact]
    public void Failure_ShouldCreateFailureError()
    {
        // Arrange
        var code = "Test.Failure";
        var description = "Test failure description";

        // Act
        var error = Error.Failure(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Unexpected_ShouldCreateUnexpectedError()
    {
        // Arrange
        var code = "Test.Unexpected";
        var description = "Test unexpected error";

        // Act
        var error = Error.Unexpected(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Unexpected);
    }

    [Fact]
    public void Validation_ShouldCreateValidationError()
    {
        // Arrange
        var code = "Test.Validation";
        var description = "Test validation error";

        // Act
        var error = Error.Validation(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Conflict_ShouldCreateConflictError()
    {
        // Arrange
        var code = "Test.Conflict";
        var description = "Test conflict error";

        // Act
        var error = Error.Conflict(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void NotFound_ShouldCreateNotFoundError()
    {
        // Arrange
        var code = "Test.NotFound";
        var description = "Test not found error";

        // Act
        var error = Error.NotFound(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Unauthorized_ShouldCreateUnauthorizedError()
    {
        // Arrange
        var code = "Test.Unauthorized";
        var description = "Test unauthorized error";

        // Act
        var error = Error.Unauthorized(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public void Forbidden_ShouldCreateForbiddenError()
    {
        // Arrange
        var code = "Test.Forbidden";
        var description = "Test forbidden error";

        // Act
        var error = Error.Forbidden(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
        error.Type.Should().Be(ErrorType.Forbidden);
    }

    #endregion

    #region Error Equality Tests

    [Fact]
    public void Errors_WithSameValues_ShouldBeEqual()
    {
        // Arrange
        var error1 = Error.Failure("Test.Error", "Description");
        var error2 = Error.Failure("Test.Error", "Description");

        // Act & Assert
        error1.Should().Be(error2);
        (error1 == error2).Should().BeTrue();
    }

    [Fact]
    public void Errors_WithDifferentCodes_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = Error.Failure("Test.Error1", "Description");
        var error2 = Error.Failure("Test.Error2", "Description");

        // Act & Assert
        error1.Should().NotBe(error2);
        (error1 != error2).Should().BeTrue();
    }

    [Fact]
    public void Errors_WithDifferentDescriptions_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = Error.Failure("Test.Error", "Description1");
        var error2 = Error.Failure("Test.Error", "Description2");

        // Act & Assert
        error1.Should().NotBe(error2);
    }

    [Fact]
    public void Errors_WithDifferentTypes_ShouldNotBeEqual()
    {
        // Arrange
        var error1 = new Error("Test.Error", "Description", ErrorType.Failure);
        var error2 = new Error("Test.Error", "Description", ErrorType.Validation);

        // Act & Assert
        error1.Should().NotBe(error2);
    }

    #endregion

    #region Implicit Conversion Tests

    [Fact]
    public void ImplicitConversion_FromErrorToResult_ShouldCreateFailureResult()
    {
        // Arrange
        var error = Error.Failure("Test.Error", "Description");

        // Act
        Result result = error;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    #endregion

    #region Edge Case Tests

    [Theory]
    [InlineData("", "Description")]
    [InlineData("Code", "")]
    [InlineData("", "")]
    public void Error_WithEmptyStrings_ShouldBeValid(string code, string description)
    {
        // Act
        var error = new Error(code, description);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
    }

    [Fact]
    public void Error_WithNullDescription_ShouldBeValid()
    {
        // Arrange
        var code = "Test.Error";

        // Act
        var error = new Error(code, null);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Unexpected)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Unauthorized)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Custom)]
    public void Error_WithAllErrorTypes_ShouldPreserveType(ErrorType errorType)
    {
        // Act
        var error = new Error("Test.Error", "Description", errorType);

        // Assert
        error.Type.Should().Be(errorType);
    }

    #endregion
}
