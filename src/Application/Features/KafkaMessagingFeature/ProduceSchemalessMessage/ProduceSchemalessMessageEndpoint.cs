using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Constants;
using CleanArchitecture.VerticalSlice.Application.Extensions;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;

namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemalessMessage;

internal sealed class ProduceSchemalessMessageEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("kafka/produce/schemaless", async (
            IHandler<ProduceSchemalessMessageRequest, Result<ProduceSchemalessMessageResponse>> handler,
            ProduceSchemalessMessageRequest request,
            CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(request, cancellationToken);
            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Kafka)
        .WithName("ProduceSchemalessMessage")
        .WithSummary("Produce a schemaless message to Kafka")
        .WithDescription("Sends a flexible key-value message to the specified Kafka topic without schema validation")
        .Produces<ProduceSchemalessMessageResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
