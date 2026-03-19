using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Constants;
using CleanArchitecture.VerticalSlice.Application.Extensions;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;

namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ConsumeMessages;

internal sealed class ConsumeMessagesEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("kafka/consume/{topic}", async (
            string topic,
            string consumerGroupId,
            int maxMessages,
            IHandler<ConsumeMessagesRequest, Result<ConsumeMessagesResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            var request = new ConsumeMessagesRequest(
                Topic: topic,
                ConsumerGroupId: consumerGroupId,
                UseSchema: false,
                MaxMessages: maxMessages);

            var result = await handler.HandleAsync(request, cancellationToken);
            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Kafka)
        .WithName("ConsumeMessages")
        .WithSummary("Consume messages from Kafka topic")
        .WithDescription("Consumes a specified number of messages from the given Kafka topic")
        .Produces<ConsumeMessagesResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
