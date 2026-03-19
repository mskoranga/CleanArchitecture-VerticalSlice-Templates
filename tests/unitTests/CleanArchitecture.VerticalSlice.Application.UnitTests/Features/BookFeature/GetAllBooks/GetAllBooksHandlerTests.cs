using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.GetAllBooks;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Data;
using CleanArchitecture.VerticalSlice.Domain.Entities;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.BookFeature.GetAllBooks;

/// <summary>
/// Unit tests for GetAllBooksHandler
/// Tests retrieving all books including empty collections
/// </summary>
public class GetAllBooksHandlerTests
{
    private readonly Mock<IRepository<Book>> _mockRepository;
    private readonly GetAllBooksHandler _handler;

    public GetAllBooksHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Book>>();
        _handler = new GetAllBooksHandler(_mockRepository.Object);
    }

    #region Success Tests - With Books

    [Fact]
    public async Task HandleAsync_WithMultipleBooks_ShouldReturnAllBooks()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), Title = "Book 1", Author = "Author 1", ISBN = "ISBN1", Price = 10m, PublishedYear = 2020 },
            new() { Id = Guid.NewGuid(), Title = "Book 2", Author = "Author 2", ISBN = "ISBN2", Price = 20m, PublishedYear = 2021 },
            new() { Id = Guid.NewGuid(), Title = "Book 3", Author = "Author 3", ISBN = "ISBN3", Price = 30m, PublishedYear = 2022 }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Books.Should().HaveCount(3);
        result.Value.Books.Should().Contain(b => b.Title == "Book 1");
        result.Value.Books.Should().Contain(b => b.Title == "Book 2");
        result.Value.Books.Should().Contain(b => b.Title == "Book 3");
    }

    [Fact]
    public async Task HandleAsync_WithSingleBook_ShouldReturnOneBook()
    {
        // Arrange
        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "978-0132350884",
            Price = 29.99m,
            PublishedYear = 2008
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book> { book });

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Books.Should().HaveCount(1);
        var returnedBook = result.Value.Books.First();
        returnedBook.Id.Should().Be(book.Id);
        returnedBook.Title.Should().Be(book.Title);
        returnedBook.Author.Should().Be(book.Author);
        returnedBook.ISBN.Should().Be(book.ISBN);
        returnedBook.Price.Should().Be(book.Price);
        returnedBook.PublishedYear.Should().Be(book.PublishedYear);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAllBookProperties()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var books = new List<Book>
        {
            new()
            {
                Id = bookId,
                Title = "Test Book",
                Author = "Test Author",
                ISBN = "123456789",
                Price = 19.99m,
                PublishedYear = 2024
            }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var dto = result.Value.Books.First();
        dto.Id.Should().Be(bookId);
        dto.Title.Should().Be("Test Book");
        dto.Author.Should().Be("Test Author");
        dto.ISBN.Should().Be("123456789");
        dto.Price.Should().Be(19.99m);
        dto.PublishedYear.Should().Be(2024);
    }

    #endregion

    #region Success Tests - Empty Collection

    [Fact]
    public async Task HandleAsync_WithNoBooks_ShouldReturnEmptyCollection()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Books.Should().BeEmpty();
        result.Value.Books.Should().HaveCount(0);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryGetAllAsync()
    {
        // Arrange
        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());

        var request = new GetAllBooksRequest();

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockRepository.Setup(x => x.GetAllAsync(token))
            .ReturnsAsync(new List<Book>());

        var request = new GetAllBooksRequest();

        // Act
        await _handler.HandleAsync(request, token);

        // Assert
        _mockRepository.Verify(
            x => x.GetAllAsync(token),
            Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var request = new GetAllBooksRequest();

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region Large Dataset Tests

    [Theory]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public async Task HandleAsync_WithLargeNumberOfBooks_ShouldReturnAllBooks(int count)
    {
        // Arrange
        var books = Enumerable.Range(1, count)
            .Select(i => new Book
            {
                Id = Guid.NewGuid(),
                Title = $"Book {i}",
                Author = $"Author {i}",
                ISBN = $"ISBN{i}",
                Price = 10m + i,
                PublishedYear = 2000 + (i % 25)
            })
            .ToList();

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Books.Should().HaveCount(count);
    }

    #endregion

    #region Data Integrity Tests

    [Fact]
    public async Task HandleAsync_ShouldPreserveBookOrder()
    {
        // Arrange
        var book1Id = Guid.NewGuid();
        var book2Id = Guid.NewGuid();
        var book3Id = Guid.NewGuid();

        var books = new List<Book>
        {
            new() { Id = book1Id, Title = "First", Author = "A1", ISBN = "1", Price = 10m, PublishedYear = 2020 },
            new() { Id = book2Id, Title = "Second", Author = "A2", ISBN = "2", Price = 20m, PublishedYear = 2021 },
            new() { Id = book3Id, Title = "Third", Author = "A3", ISBN = "3", Price = 30m, PublishedYear = 2022 }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        var resultList = result.Value.Books.ToList();
        resultList[0].Id.Should().Be(book1Id);
        resultList[1].Id.Should().Be(book2Id);
        resultList[2].Id.Should().Be(book3Id);
    }

    [Fact]
    public async Task HandleAsync_WithDuplicateData_ShouldReturnAllEntries()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), Title = "Same Title", Author = "Same Author", ISBN = "ISBN1", Price = 10m, PublishedYear = 2020 },
            new() { Id = Guid.NewGuid(), Title = "Same Title", Author = "Same Author", ISBN = "ISBN2", Price = 10m, PublishedYear = 2020 },
            new() { Id = Guid.NewGuid(), Title = "Same Title", Author = "Same Author", ISBN = "ISBN3", Price = 10m, PublishedYear = 2020 }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Books.Should().HaveCount(3);
        result.Value.Books.Select(b => b.ISBN).Should().BeEquivalentTo(new[] { "ISBN1", "ISBN2", "ISBN3" });
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task HandleAsync_WithBooksHavingExtremeValues_ShouldHandleCorrectly()
    {
        // Arrange
        var books = new List<Book>
        {
            new() { Id = Guid.NewGuid(), Title = "A", Author = "B", ISBN = "1", Price = 0.01m, PublishedYear = 1000 },
            new() { Id = Guid.NewGuid(), Title = new string('X', 200), Author = new string('Y', 100), ISBN = "999", Price = 99999.99m, PublishedYear = DateTime.UtcNow.Year }
        };

        _mockRepository.Setup(x => x.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var request = new GetAllBooksRequest();

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Books.Should().HaveCount(2);
    }

    #endregion
}
