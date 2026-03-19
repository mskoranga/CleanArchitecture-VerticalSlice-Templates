using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.GetBookById;
using CleanArchitecture.VerticalSlice.Application.Features.BookFeature;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Data;
using CleanArchitecture.VerticalSlice.Domain.Entities;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.BookFeature.GetBookById;

/// <summary>
/// Unit tests for GetBookByIdHandler
/// Tests successful retrieval and not found scenarios
/// </summary>
public class GetBookByIdHandlerTests
{
    private readonly Mock<IRepository<Book>> _mockRepository;
    private readonly GetBookByIdHandler _handler;

    public GetBookByIdHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Book>>();
        _handler = new GetBookByIdHandler(_mockRepository.Object);
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithExistingBook_ShouldReturnSuccessResult()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book
        {
            Id = bookId,
            Title = "Clean Code",
            Author = "Robert C. Martin",
            ISBN = "978-0132350884",
            Price = 29.99m,
            PublishedYear = 2008
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new GetBookByIdRequest(bookId);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(bookId);
        result.Value.Title.Should().Be(book.Title);
        result.Value.Author.Should().Be(book.Author);
        result.Value.ISBN.Should().Be(book.ISBN);
        result.Value.Price.Should().Be(book.Price);
        result.Value.PublishedYear.Should().Be(book.PublishedYear);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryGetByIdAsync()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book
        {
            Id = bookId,
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "123456789",
            Price = 19.99m,
            PublishedYear = 2024
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new GetBookByIdRequest(bookId);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion

    #region Not Found Tests

    [Fact]
    public async Task HandleAsync_WithNonExistingBook_ShouldReturnFailureResult()
    {
        // Arrange
        var bookId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var request = new GetBookByIdRequest(bookId);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().NotBeNull();
        result.Error.Code.Should().Be("Books.NotFound");
        result.Error.Description.Should().Contain(bookId.ToString());
    }

    [Fact]
    public async Task HandleAsync_WithNullBook_ShouldReturnNotFoundError()
    {
        // Arrange
        var bookId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var request = new GetBookByIdRequest(bookId);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookErrors.NotFound(bookId));
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var book = new Book
        {
            Id = bookId,
            Title = "Test",
            Author = "Author",
            ISBN = "ISBN",
            Price = 10m,
            PublishedYear = 2024
        };

        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, token))
            .ReturnsAsync(book);

        var request = new GetBookByIdRequest(bookId);

        // Act
        await _handler.HandleAsync(request, token);

        // Assert
        _mockRepository.Verify(
            x => x.GetByIdAsync(bookId, token),
            Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var request = new GetBookByIdRequest(bookId);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task HandleAsync_WithEmptyGuid_ShouldReturnNotFound()
    {
        // Arrange
        var emptyGuid = Guid.Empty;

        _mockRepository.Setup(x => x.GetByIdAsync(emptyGuid, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var request = new GetBookByIdRequest(emptyGuid);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Books.NotFound");
    }

    [Theory]
    [MemberData(nameof(GetMultipleBooks))]
    public async Task HandleAsync_WithDifferentBooks_ShouldReturnCorrectBook(Book book)
    {
        // Arrange
        _mockRepository.Setup(x => x.GetByIdAsync(book.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        var request = new GetBookByIdRequest(book.Id);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(book.Id);
        result.Value.Title.Should().Be(book.Title);
    }

    public static IEnumerable<object[]> GetMultipleBooks()
    {
        yield return new object[]
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book 1",
                Author = "Author 1",
                ISBN = "ISBN1",
                Price = 10m,
                PublishedYear = 2020
            }
        };

        yield return new object[]
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book 2",
                Author = "Author 2",
                ISBN = "ISBN2",
                Price = 20m,
                PublishedYear = 2021
            }
        };

        yield return new object[]
        {
            new Book
            {
                Id = Guid.NewGuid(),
                Title = "Book 3",
                Author = "Author 3",
                ISBN = "ISBN3",
                Price = 30m,
                PublishedYear = 2022
            }
        };
    }

    #endregion
}
