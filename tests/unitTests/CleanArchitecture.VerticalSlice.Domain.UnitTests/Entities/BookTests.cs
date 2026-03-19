using CleanArchitecture.VerticalSlice.Domain.Entities;

namespace CleanArchitecture.VerticalSlice.Domain.UnitTests.Entities;

/// <summary>
/// Unit tests for Book entity
/// Tests entity creation, validation, and property assignments
/// </summary>
public class BookTests
{
    #region Creation Tests

    [Fact]
    public void Book_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        var id = Guid.NewGuid();
        var title = "Clean Code";
        var author = "Robert C. Martin";
        var isbn = "978-0132350884";
        var price = 29.99m;
        var publishedYear = 2008;

        // Act
        var book = new Book
        {
            Id = id,
            Title = title,
            Author = author,
            ISBN = isbn,
            Price = price,
            PublishedYear = publishedYear
        };

        // Assert
        book.Id.Should().Be(id);
        book.Title.Should().Be(title);
        book.Author.Should().Be(author);
        book.ISBN.Should().Be(isbn);
        book.Price.Should().Be(price);
        book.PublishedYear.Should().Be(publishedYear);
    }

    [Fact]
    public void Book_ShouldInheritFromAuditableEntity()
    {
        // Arrange & Act
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "123456789",
            Price = 10m,
            PublishedYear = 2024
        };

        // Assert
        book.Should().BeAssignableTo<AuditableEntity>();
    }

    #endregion

    #region Property Validation Tests

    [Theory]
    [InlineData("A")]
    [InlineData("Test Book with a Very Long Title")]
    [InlineData("Book")]
    public void Book_WithVariousTitleLengths_ShouldAccept(string title)
    {
        // Act
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = title,
            Author = "Author",
            ISBN = "123",
            Price = 10m,
            PublishedYear = 2024
        };

        // Assert
        book.Title.Should().Be(title);
    }

    [Theory]
    [InlineData(0.01)]
    [InlineData(9.99)]
    [InlineData(99.99)]
    [InlineData(999.99)]
    public void Book_WithVariousPrices_ShouldAccept(decimal price)
    {
        // Act
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Author = "Author",
            ISBN = "123",
            Price = price,
            PublishedYear = 2024
        };

        // Assert
        book.Price.Should().Be(price);
    }

    [Theory]
    [InlineData(1000)]
    [InlineData(2000)]
    [InlineData(2024)]
    public void Book_WithVariousPublishedYears_ShouldAccept(int year)
    {
        // Act
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Author = "Author",
            ISBN = "123",
            Price = 10m,
            PublishedYear = year
        };

        // Assert
        book.PublishedYear.Should().Be(year);
    }

    #endregion

    #region ISBN Format Tests

    [Theory]
    [InlineData("978-0132350884")]
    [InlineData("0-13-235088-2")]
    [InlineData("1234567890")]
    [InlineData("ISBN-123456")]
    public void Book_WithVariousISBNFormats_ShouldAccept(string isbn)
    {
        // Act
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Test",
            Author = "Author",
            ISBN = isbn,
            Price = 10m,
            PublishedYear = 2024
        };

        // Assert
        book.ISBN.Should().Be(isbn);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Book_UpdateTitle_ShouldReflectNewValue()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Author = "Author",
            ISBN = "123",
            Price = 10m,
            PublishedYear = 2024
        };

        // Act
        book.Title = "Updated Title";

        // Assert
        book.Title.Should().Be("Updated Title");
    }

    [Fact]
    public void Book_UpdateAuthor_ShouldReflectNewValue()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Author = "Original Author",
            ISBN = "123",
            Price = 10m,
            PublishedYear = 2024
        };

        // Act
        book.Author = "Updated Author";

        // Assert
        book.Author.Should().Be("Updated Author");
    }

    [Fact]
    public void Book_UpdatePrice_ShouldReflectNewValue()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Title",
            Author = "Author",
            ISBN = "123",
            Price = 10m,
            PublishedYear = 2024
        };

        // Act
        book.Price = 20m;

        // Assert
        book.Price.Should().Be(20m);
    }

    [Fact]
    public void Book_UpdateAllProperties_ShouldReflectAllChanges()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Original Title",
            Author = "Original Author",
            ISBN = "123",
            Price = 10m,
            PublishedYear = 2024
        };

        // Act
        book.Title = "New Title";
        book.Author = "New Author";
        book.ISBN = "456";
        book.Price = 25m;
        book.PublishedYear = 2025;

        // Assert
        book.Title.Should().Be("New Title");
        book.Author.Should().Be("New Author");
        book.ISBN.Should().Be("456");
        book.Price.Should().Be(25m);
        book.PublishedYear.Should().Be(2025);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Book_WithZeroPrice_ShouldAccept()
    {
        // Act
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Free Book",
            Author = "Author",
            ISBN = "123",
            Price = 0m,
            PublishedYear = 2024
        };

        // Assert
        book.Price.Should().Be(0m);
    }

    [Fact]
    public void Book_WithNegativePrice_ShouldAccept()
    {
        // Note: Entity allows negative price, validation should be handled at application layer
        // Act
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Book",
            Author = "Author",
            ISBN = "123",
            Price = -10m,
            PublishedYear = 2024
        };

        // Assert
        book.Price.Should().Be(-10m);
    }

    [Fact]
    public void Book_WithVersion7Guid_ShouldHaveUniqueId()
    {
        // Arrange
        var id1 = Guid.CreateVersion7();
        var id2 = Guid.CreateVersion7();

        // Act
        var book1 = new Book
        {
            Id = id1,
            Title = "Book 1",
            Author = "Author",
            ISBN = "123",
            Price = 10m,
            PublishedYear = 2024
        };

        var book2 = new Book
        {
            Id = id2,
            Title = "Book 2",
            Author = "Author",
            ISBN = "456",
            Price = 15m,
            PublishedYear = 2024
        };

        // Assert
        book1.Id.Should().NotBe(book2.Id);
        id1.Should().NotBe(id2);
    }

    #endregion
}
