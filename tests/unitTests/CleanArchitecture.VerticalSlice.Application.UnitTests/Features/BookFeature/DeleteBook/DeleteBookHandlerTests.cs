using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.DeleteBook;
using CleanArchitecture.VerticalSlice.Application.Features.BookFeature;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Data;
using CleanArchitecture.VerticalSlice.Domain.Entities;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.BookFeature.DeleteBook;

/// <summary>
/// Unit tests for DeleteBookHandler
/// Tests successful deletion and not found scenarios
/// </summary>
public class DeleteBookHandlerTests
{
    private readonly Mock<IRepository<Book>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly DeleteBookHandler _handler;

    public DeleteBookHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Book>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new DeleteBookHandler(_mockRepository.Object, _mockUnitOfWork.Object);
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
            Title = "Test Book",
            Author = "Test Author",
            ISBN = "123456789",
            Price = 19.99m,
            PublishedYear = 2024
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _mockRepository.Setup(x => x.DeleteAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new DeleteBookRequest(bookId);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(bookId);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryDeleteAsync()
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

        _mockRepository.Setup(x => x.DeleteAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new DeleteBookRequest(bookId);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.DeleteAsync(
                It.Is<Book>(b => b.Id == bookId),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallUnitOfWorkCommitAsync()
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

        _mockRepository.Setup(x => x.DeleteAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new DeleteBookRequest(bookId);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
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

        var request = new DeleteBookRequest(bookId);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
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

        var request = new DeleteBookRequest(bookId);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(BookErrors.NotFound(bookId));
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingBook_ShouldNotCallDeleteOrCommit()
    {
        // Arrange
        var bookId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var request = new DeleteBookRequest(bookId);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.DeleteAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
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

        _mockRepository.Setup(x => x.DeleteAsync(book, token))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(token))
            .ReturnsAsync(1);

        var request = new DeleteBookRequest(bookId);

        // Act
        await _handler.HandleAsync(request, token);

        // Assert
        _mockRepository.Verify(x => x.GetByIdAsync(bookId, token), Times.Once);
        _mockRepository.Verify(x => x.DeleteAsync(book, token), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(token), Times.Once);
    }

    #endregion

    #region Exception Handling Tests

    [Fact]
    public async Task HandleAsync_WhenRepositoryGetByIdThrowsException_ShouldPropagateException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var expectedException = new InvalidOperationException("Database error");

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var request = new DeleteBookRequest(bookId);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryDeleteThrowsException_ShouldPropagateException()
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

        var expectedException = new InvalidOperationException("Delete failed");

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _mockRepository.Setup(x => x.DeleteAsync(book, It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var request = new DeleteBookRequest(bookId);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Delete failed");
    }

    [Fact]
    public async Task HandleAsync_WhenUnitOfWorkThrowsException_ShouldPropagateException()
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

        var expectedException = new InvalidOperationException("Commit failed");

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(book);

        _mockRepository.Setup(x => x.DeleteAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var request = new DeleteBookRequest(bookId);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Commit failed");
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

        var request = new DeleteBookRequest(emptyGuid);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Books.NotFound");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task HandleAsync_MultipleDeletions_ShouldHandleEachDeletion(int count)
    {
        // Arrange & Act & Assert
        for (int i = 0; i < count; i++)
        {
            var bookId = Guid.NewGuid();
            var book = new Book
            {
                Id = bookId,
                Title = $"Book {i}",
                Author = "Author",
                ISBN = $"ISBN{i}",
                Price = 10m + i,
                PublishedYear = 2024
            };

            _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(book);

            _mockRepository.Setup(x => x.DeleteAsync(book, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var request = new DeleteBookRequest(bookId);
            var result = await _handler.HandleAsync(request, CancellationToken.None);

            result.IsSuccess.Should().BeTrue();
            result.Value.Id.Should().Be(bookId);
        }
    }

    #endregion
}
