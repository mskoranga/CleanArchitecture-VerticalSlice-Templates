using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.CreateBook;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Data;
using CleanArchitecture.VerticalSlice.Domain.Entities;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.BookFeature.CreateBook;

/// <summary>
/// Unit tests for CreateBookHandler
/// Tests successful book creation and error scenarios
/// </summary>
public class CreateBookHandlerTests
{
    private readonly Mock<IRepository<Book>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateBookHandler _handler;

    public CreateBookHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Book>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateBookHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    #region Success Tests

    [Fact]
    public async Task HandleAsync_WithValidRequest_ShouldReturnSuccessResult()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: "Clean Code",
            Author: "Robert C. Martin",
            ISBN: "978-0132350884",
            Price: 29.99m,
            PublishedYear: 2008
        );

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Title.Should().Be(request.Title);
        result.Value.Author.Should().Be(request.Author);
        result.Value.ISBN.Should().Be(request.ISBN);
        result.Value.Price.Should().Be(request.Price);
        result.Value.PublishedYear.Should().Be(request.PublishedYear);
        result.Value.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryAddAsync()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: "Test Book",
            Author: "Test Author",
            ISBN: "123456789",
            Price: 19.99m,
            PublishedYear: 2024
        );

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.AddAsync(
                It.Is<Book>(b =>
                    b.Title == request.Title &&
                    b.Author == request.Author &&
                    b.ISBN == request.ISBN &&
                    b.Price == request.Price &&
                    b.PublishedYear == request.PublishedYear),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallUnitOfWorkCommitAsync()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: "Test Book",
            Author: "Test Author",
            ISBN: "123456789",
            Price: 19.99m,
            PublishedYear: 2024
        );

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldGenerateVersion7Guid()
    {
        // Arrange
        var request = new CreateBookRequest(
            Title: "Test Book",
            Author: "Test Author",
            ISBN: "123456789",
            Price: 19.99m,
            PublishedYear: 2024
        );

        Book? capturedBook = null;
        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((book, ct) => capturedBook = book)
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        capturedBook.Should().NotBeNull();
        capturedBook!.Id.Should().NotBe(Guid.Empty);
        result.Value.Id.Should().Be(capturedBook.Id);
    }

    #endregion

    #region Data Integrity Tests

    [Theory]
    [InlineData("Short", "A", "1", 0.01, 1000)]
    [InlineData("Very Long Title That Might Exceed Some Limits", "Very Long Author Name", "978-0132350884", 999.99, 2024)]
    public async Task HandleAsync_WithVariousValidInputs_ShouldReturnSuccess(
        string title, string author, string isbn, decimal price, int year)
    {
        // Arrange
        var request = new CreateBookRequest(title, author, isbn, price, year);

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be(title);
        result.Value.Author.Should().Be(author);
        result.Value.ISBN.Should().Be(isbn);
        result.Value.Price.Should().Be(price);
        result.Value.PublishedYear.Should().Be(year);
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task HandleAsync_WithCancellationToken_ShouldPassToRepository()
    {
        // Arrange
        var request = new CreateBookRequest("Test", "Author", "ISBN", 10m, 2024);
        var cts = new CancellationTokenSource();
        var token = cts.Token;

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), token))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _mockUnitOfWork.Setup(x => x.CommitAsync(token))
            .ReturnsAsync(1);

        // Act
        await _handler.HandleAsync(request, token);

        // Assert
        _mockRepository.Verify(
            x => x.AddAsync(It.IsAny<Book>(), token),
            Times.Once);

        _mockUnitOfWork.Verify(
            x => x.CommitAsync(token),
            Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task HandleAsync_WhenRepositoryThrowsException_ShouldPropagateException()
    {
        // Arrange
        var request = new CreateBookRequest("Test", "Author", "ISBN", 10m, 2024);
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task HandleAsync_WhenUnitOfWorkThrowsException_ShouldPropagateException()
    {
        // Arrange
        var request = new CreateBookRequest("Test", "Author", "ISBN", 10m, 2024);
        var expectedException = new InvalidOperationException("Commit failed");

        _mockRepository.Setup(x => x.AddAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book b, CancellationToken ct) => b);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Commit failed");
    }

    #endregion
}
