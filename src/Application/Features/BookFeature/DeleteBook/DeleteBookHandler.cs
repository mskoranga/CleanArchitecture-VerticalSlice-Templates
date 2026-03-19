namespace CleanArchitecture.VerticalSlice.Application.Features.BookFeature.DeleteBook;

using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Abstractions.Data;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;
using CleanArchitecture.VerticalSlice.Domain.Entities;

public sealed record DeleteBookRequest(Guid Id);
public sealed record DeleteBookResponse(Guid Id);

public sealed class DeleteBookHandler(
    IRepository<Book> _bookRepo,
    IUnitOfWork _unitOfWork) : IHandler<DeleteBookRequest, Result<DeleteBookResponse>>
{
    public async Task<Result<DeleteBookResponse>> HandleAsync(DeleteBookRequest command, CancellationToken cancellationToken)
    {
        var book = await _bookRepo.GetByIdAsync(command.Id, cancellationToken);

        if (book == null)
        {
            return Result.Failure<DeleteBookResponse>(BookErrors.NotFound(command.Id));
        }

        await _bookRepo.DeleteAsync(book, cancellationToken);
        await _unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new DeleteBookResponse(book.Id));
    }
}
