using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.UpdateBook;
using FluentValidation.TestHelper;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.BookFeature.UpdateBook;

/// <summary>
/// Unit tests for UpdateBookValidator
/// Tests validation rules for update operations including optional fields
/// </summary>
public class UpdateBookValidatorTests
{
    private readonly UpdateBookValidator _validator;

    public UpdateBookValidatorTests()
    {
        _validator = new UpdateBookValidator();
    }

    #region Valid Scenarios

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(
            Id: Guid.NewGuid(),
            Title: "Clean Code",
            Author: "Robert C. Martin",
            ISBN: "978-0132350884",
            Price: 29.99m,
            PublishedYear: 2008
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithAllNullFields_ShouldNotHaveError()
    {
        // Arrange - All optional fields are null (only updating ID)
        var request = new UpdateBookRequest(
            Id: Guid.NewGuid(),
            Title: null,
            Author: null,
            ISBN: null,
            Price: null,
            PublishedYear: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPartialUpdate_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(
            Id: Guid.NewGuid(),
            Title: "New Title",
            Author: null,
            ISBN: null,
            Price: null,
            PublishedYear: null
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Title Validation

    [Fact]
    public void Validate_WithValidTitle_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), "Valid Title", null, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyTitle_ShouldHaveError(string title)
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), title, null, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithTitleExceeding200Characters_ShouldHaveError()
    {
        // Arrange
        var longTitle = new string('A', 201);
        var request = new UpdateBookRequest(Guid.NewGuid(), longTitle, null, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    #endregion

    #region Author Validation

    [Fact]
    public void Validate_WithValidAuthor_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, "Robert C. Martin", null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Author);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyAuthor_ShouldHaveError(string author)
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, author, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public void Validate_WithAuthorExceeding100Characters_ShouldHaveError()
    {
        // Arrange
        var longAuthor = new string('A', 101);
        var request = new UpdateBookRequest(Guid.NewGuid(), null, longAuthor, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Author)
            .WithErrorMessage("Author must not exceed 100 characters");
    }

    #endregion

    #region ISBN Validation

    [Fact]
    public void Validate_WithValidISBN_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, "978-0132350884", null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ISBN);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyISBN_ShouldHaveError(string isbn)
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, isbn, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ISBN);
    }

    #endregion

    #region Price Validation

    [Theory]
    [InlineData(0.01)]
    [InlineData(10.00)]
    [InlineData(999.99)]
    public void Validate_WithValidPrice_ShouldNotHaveError(decimal price)
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, null, price, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-10)]
    public void Validate_WithInvalidPrice_ShouldHaveError(decimal price)
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, null, price, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than 0");
    }

    #endregion

    #region PublishedYear Validation

    [Theory]
    [InlineData(1001)]
    [InlineData(2000)]
    [InlineData(2024)]
    public void Validate_WithValidPublishedYear_ShouldNotHaveError(int year)
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, null, null, year);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Fact]
    public void Validate_WithYearLessThan1000_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, null, null, 999);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear)
            .WithErrorMessage("Published year must be a valid year");
    }

    [Fact]
    public void Validate_WithFutureYear_ShouldHaveError()
    {
        // Arrange
        var futureYear = DateTime.UtcNow.Year + 1;
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, null, null, futureYear);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear)
            .WithErrorMessage("Published year cannot be in the future");
    }

    #endregion

    #region ID Validation

    [Fact]
    public void Validate_WithValidId_ShouldNotHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.NewGuid(), null, null, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Validate_WithEmptyGuid_ShouldHaveError()
    {
        // Arrange
        var request = new UpdateBookRequest(Guid.Empty, null, null, null, null, null);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    #endregion
}
