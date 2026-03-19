using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Constants;
using CleanArchitecture.VerticalSlice.Application.Extensions;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;

namespace CleanArchitecture.VerticalSlice.Application.Features.BookFeature.GetBookById;

internal sealed class GetBookByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("books/{id:guid}", async (Guid id, IHandler<GetBookByIdRequest, Result<GetBookByIdResponse>> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetBookByIdRequest(id), cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.NotFound(error));
        })
        .WithTags(ApiTags.Books)
        .Produces<GetBookByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
