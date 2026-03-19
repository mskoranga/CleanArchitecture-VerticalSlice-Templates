using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.CreateBook;
using FluentValidation.TestHelper;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.BookFeature.CreateBook;

/// <summary>
/// Unit tests for CreateBookValidator
/// Tests all validation rules for positive and negative scenarios
/// </summary>
public class CreateBookValidatorTests
{
    private readonly CreateBookValidator _validator;

    public CreateBookValidatorTests()
    {
        _validator = new CreateBookValidator();
    }

    #region Title Validation Tests

    [Fact]
    public void Validate_WithValidTitle_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: "Clean Code",
            Author: "Robert C. Martin",
            ISBN: "978-0132350884",
            Price: 29.99m,
            PublishedYear: 2008
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Validate_WithEmptyTitle_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: string.Empty,
            Author: "Author",
            ISBN: "123",
            Price: 10m,
            PublishedYear: 2024
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required");
    }

    [Fact]
    public void Validate_WithNullTitle_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: null!,
            Author: "Author",
            ISBN: "123",
            Price: 10m,
            PublishedYear: 2024
        );

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
        var request = new CreateBookRequest(
            Title: longTitle,
            Author: "Author",
            ISBN: "123",
            Price: 10m,
            PublishedYear: 2024
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 200 characters");
    }

    [Fact]
    public void Validate_WithTitleExactly200Characters_ShouldNotHaveError()
    {
        // Arrange
        var title = new string('A', 200);
        var request = new CreateBookRequest(
            Title: title,
            Author: "Author",
            ISBN: "123",
            Price: 10m,
            PublishedYear: 2024
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Theory]
    [InlineData("A")]
    [InlineData("AB")]
    [InlineData("Short Title")]
    public void Validate_WithShortValidTitles_ShouldNotHaveError(string title)
    {
        // Arrange
        var request = new CreateBookRequest(title, "Author", "123", 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    #endregion

    #region Author Validation Tests

    [Fact]
    public void Validate_WithValidAuthor_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Robert C. Martin", "123", 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Author);
    }

    [Fact]
    public void Validate_WithEmptyAuthor_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", string.Empty, "123", 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Author)
            .WithErrorMessage("Author is required");
    }

    [Fact]
    public void Validate_WithNullAuthor_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", null!, "123", 10m, 2024);

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
        var request = new CreateBookRequest("Title", longAuthor, "123", 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Author)
            .WithErrorMessage("Author must not exceed 100 characters");
    }

    [Fact]
    public void Validate_WithAuthorExactly100Characters_ShouldNotHaveError()
    {
        // Arrange
        var author = new string('A', 100);
        var request = new CreateBookRequest("Title", author, "123", 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Author);
    }

    #endregion

    #region ISBN Validation Tests

    [Fact]
    public void Validate_WithValidISBN_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", "978-0132350884", 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ISBN);
    }

    [Fact]
    public void Validate_WithEmptyISBN_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", string.Empty, 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ISBN)
            .WithErrorMessage("ISBN is required");
    }

    [Fact]
    public void Validate_WithNullISBN_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", null!, 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ISBN);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("123456789")]
    [InlineData("978-0132350884")]
    [InlineData("0-13-235088-2")]
    public void Validate_WithVariousISBNFormats_ShouldNotHaveError(string isbn)
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", isbn, 10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ISBN);
    }

    #endregion

    #region Price Validation Tests

    [Fact]
    public void Validate_WithValidPrice_ShouldNotHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", "123", 29.99m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    [Fact]
    public void Validate_WithZeroPrice_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", "123", 0m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than 0");
    }

    [Fact]
    public void Validate_WithNegativePrice_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", "123", -10m, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Price)
            .WithErrorMessage("Price must be greater than 0");
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(1.00)]
    [InlineData(99.99)]
    [InlineData(999.99)]
    public void Validate_WithVariousValidPrices_ShouldNotHaveError(decimal price)
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", "123", price, 2024);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Price);
    }

    #endregion

    #region PublishedYear Validation Tests

    [Fact]
    public void Validate_WithValidPublishedYear_ShouldNotHaveError()
    {
        // Arrange
        var currentYear = DateTime.UtcNow.Year;
        var request = new CreateBookRequest("Title", "Author", "123", 10m, currentYear);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Fact]
    public void Validate_WithPublishedYearLessThan1000_ShouldHaveError()
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", "123", 10m, 999);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear)
            .WithErrorMessage("Published year must be a valid year");
    }

    [Fact]
    public void Validate_WithPublishedYearInFuture_ShouldHaveError()
    {
        // Arrange
        var futureYear = DateTime.UtcNow.Year + 1;
        var request = new CreateBookRequest("Title", "Author", "123", 10m, futureYear);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear)
            .WithErrorMessage("Published year cannot be in the future");
    }

    [Theory]
    [InlineData(1001)]
    [InlineData(1500)]
    [InlineData(2000)]
    [InlineData(2020)]
    public void Validate_WithHistoricalYears_ShouldNotHaveError(int year)
    {
        // Arrange
        var request = new CreateBookRequest("Title", "Author", "123", 10m, year);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PublishedYear);
    }

    #endregion

    #region Multiple Errors Tests

    [Fact]
    public void Validate_WithAllInvalidFields_ShouldHaveMultipleErrors()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: string.Empty,
            Author: string.Empty,
            ISBN: string.Empty,
            Price: 0m,
            PublishedYear: 999
        );

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Title);
        result.ShouldHaveValidationErrorFor(x => x.Author);
        result.ShouldHaveValidationErrorFor(x => x.ISBN);
        result.ShouldHaveValidationErrorFor(x => x.Price);
        result.ShouldHaveValidationErrorFor(x => x.PublishedYear);
    }

    [Fact]
    public void Validate_WithAllValidFields_ShouldNotHaveAnyErrors()
    {
        // Arrange
        var request = new CreateBookRequest(
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

    #endregion
}
