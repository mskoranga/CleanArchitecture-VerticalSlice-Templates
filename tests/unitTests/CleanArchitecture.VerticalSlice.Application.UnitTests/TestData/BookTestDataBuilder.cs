using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.CreateBook;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.TestData;

/// <summary>
/// Test data builder for creating test book requests
/// Follows the Builder pattern for flexible test data generation
/// </summary>
public class BookTestDataBuilder
{
    private string _title = "Default Test Book";
    private string _author = "Default Test Author";
    private string _isbn = "978-0000000000";
    private decimal _price = 19.99m;
    private int _publishedYear = DateTime.UtcNow.Year;

    public BookTestDataBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public BookTestDataBuilder WithAuthor(string author)
    {
        _author = author;
        return this;
    }

    public BookTestDataBuilder WithISBN(string isbn)
    {
        _isbn = isbn;
        return this;
    }

    public BookTestDataBuilder WithPrice(decimal price)
    {
        _price = price;
        return this;
    }

    public BookTestDataBuilder WithPublishedYear(int year)
    {
        _publishedYear = year;
        return this;
    }

    public CreateBookRequest BuildCreateRequest()
    {
        return new CreateBookRequest(
            Title: _title,
            Author: _author,
            ISBN: _isbn,
            Price: _price,
            PublishedYear: _publishedYear
        );
    }

    public static BookTestDataBuilder Create() => new();

    /// <summary>
    /// Creates a valid book request with default values
    /// </summary>
    public static CreateBookRequest CreateValidBook()
    {
        return new CreateBookRequest(
            Title: "Clean Code",
            Author: "Robert C. Martin",
            ISBN: "978-0132350884",
            Price: 29.99m,
            PublishedYear: 2008
        );
    }

    /// <summary>
    /// Creates multiple valid book requests for testing
    /// </summary>
    public static IEnumerable<CreateBookRequest> CreateMultipleValidBooks()
    {
        return new[]
        {
            new CreateBookRequest("Clean Code", "Robert C. Martin", "978-0132350884", 29.99m, 2008),
            new CreateBookRequest("The Pragmatic Programmer", "David Thomas", "978-0201616224", 39.99m, 1999),
            new CreateBookRequest("Design Patterns", "Gang of Four", "978-0201633610", 54.99m, 1994),
            new CreateBookRequest("Refactoring", "Martin Fowler", "978-0201485677", 49.99m, 1999),
            new CreateBookRequest("Domain-Driven Design", "Eric Evans", "978-0321125217", 59.99m, 2003)
        };
    }
}
