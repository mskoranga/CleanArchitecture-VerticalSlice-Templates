using CleanArchitecture.VerticalSlice.Application.Features.BookFeature.UpdateBook;
using CleanArchitecture.VerticalSlice.Application.Features.BookFeature;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Data;
using CleanArchitecture.VerticalSlice.Domain.Entities;

namespace CleanArchitecture.VerticalSlice.Application.UnitTests.Features.BookFeature.UpdateBook;

/// <summary>
/// Unit tests for UpdateBookHandler
/// Tests update scenarios including partial updates and not found cases
/// </summary>
public class UpdateBookHandlerTests
{
    private readonly Mock<IRepository<Book>> _mockRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly UpdateBookHandler _handler;

    public UpdateBookHandlerTests()
    {
        _mockRepository = new Mock<IRepository<Book>>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new UpdateBookHandler(_mockRepository.Object, _mockUnitOfWork.Object);
    }

    #region Success Tests - Full Update

    [Fact]
    public async Task HandleAsync_WithAllFieldsUpdated_ShouldReturnSuccessResult()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Old Title",
            Author = "Old Author",
            ISBN = "Old ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateBookRequest(
            Id: bookId,
            Title: "New Title",
            Author: "New Author",
            ISBN: "New ISBN",
            Price: 20m,
            PublishedYear: 2024
        );

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(bookId);
        result.Value.Title.Should().Be("New Title");
        result.Value.Author.Should().Be("New Author");
        result.Value.ISBN.Should().Be("New ISBN");
        result.Value.Price.Should().Be(20m);
        result.Value.PublishedYear.Should().Be(2024);
    }

    #endregion

    #region Success Tests - Partial Update

    [Fact]
    public async Task HandleAsync_WithOnlyTitleUpdated_ShouldUpdateOnlyTitle()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Old Title",
            Author = "Old Author",
            ISBN = "Old ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateBookRequest(
            Id: bookId,
            Title: "New Title",
            Author: null,
            ISBN: null,
            Price: null,
            PublishedYear: null
        );

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Title");
        result.Value.Author.Should().Be("Old Author");
        result.Value.ISBN.Should().Be("Old ISBN");
        result.Value.Price.Should().Be(10m);
        result.Value.PublishedYear.Should().Be(2020);
    }

    [Fact]
    public async Task HandleAsync_WithOnlyPriceUpdated_ShouldUpdateOnlyPrice()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Title",
            Author = "Author",
            ISBN = "ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateBookRequest(
            Id: bookId,
            Title: null,
            Author: null,
            ISBN: null,
            Price: 25m,
            PublishedYear: null
        );

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Price.Should().Be(25m);
        result.Value.Title.Should().Be("Title");
        result.Value.Author.Should().Be("Author");
    }

    [Fact]
    public async Task HandleAsync_WithMultipleFieldsUpdated_ShouldUpdateSpecifiedFields()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Old Title",
            Author = "Old Author",
            ISBN = "Old ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateBookRequest(
            Id: bookId,
            Title: "New Title",
            Author: null,
            ISBN: "New ISBN",
            Price: null,
            PublishedYear: 2024
        );

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("New Title");
        result.Value.Author.Should().Be("Old Author"); // Not updated
        result.Value.ISBN.Should().Be("New ISBN");
        result.Value.Price.Should().Be(10m); // Not updated
        result.Value.PublishedYear.Should().Be(2024);
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

        var request = new UpdateBookRequest(
            Id: bookId,
            Title: "New Title",
            Author: null,
            ISBN: null,
            Price: null,
            PublishedYear: null
        );

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Books.NotFound");
        result.Error.Description.Should().Contain(bookId.ToString());
    }

    [Fact]
    public async Task HandleAsync_WithNonExistingBook_ShouldNotCallUpdateOrCommit()
    {
        // Arrange
        var bookId = Guid.NewGuid();

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var request = new UpdateBookRequest(bookId, "Title", null, null, null, null);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _mockUnitOfWork.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    #endregion

    #region Repository Interaction Tests

    [Fact]
    public async Task HandleAsync_ShouldCallRepositoryUpdateAsync()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Title",
            Author = "Author",
            ISBN = "ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateBookRequest(bookId, "New Title", null, null, null, null);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockRepository.Verify(
            x => x.UpdateAsync(
                It.Is<Book>(b => b.Id == bookId && b.Title == "New Title"),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldCallUnitOfWorkCommitAsync()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Title",
            Author = "Author",
            ISBN = "ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateBookRequest(bookId, "Title", null, null, null, null);

        // Act
        await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(
            x => x.CommitAsync(It.IsAny<CancellationToken>()),
            Times.Once);
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

        var request = new UpdateBookRequest(bookId, "Title", null, null, null, null);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Database error");
    }

    [Fact]
    public async Task HandleAsync_WhenRepositoryUpdateThrowsException_ShouldPropagateException()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Title",
            Author = "Author",
            ISBN = "ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        var expectedException = new InvalidOperationException("Update failed");

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(expectedException);

        var request = new UpdateBookRequest(bookId, "New Title", null, null, null, null);

        // Act
        var act = async () => await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Update failed");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task HandleAsync_WithNoFieldsToUpdate_ShouldStillCallUpdateAndCommit()
    {
        // Arrange
        var bookId = Guid.NewGuid();
        var existingBook = new Book
        {
            Id = bookId,
            Title = "Title",
            Author = "Author",
            ISBN = "ISBN",
            Price = 10m,
            PublishedYear = 2020
        };

        _mockRepository.Setup(x => x.GetByIdAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingBook);

        _mockRepository.Setup(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUnitOfWork.Setup(x => x.CommitAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var request = new UpdateBookRequest(bookId, null, null, null, null, null);

        // Act
        var result = await _handler.HandleAsync(request, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Title.Should().Be("Title");
        result.Value.Author.Should().Be("Author");
        _mockRepository.Verify(x => x.UpdateAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.CommitAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion
}
