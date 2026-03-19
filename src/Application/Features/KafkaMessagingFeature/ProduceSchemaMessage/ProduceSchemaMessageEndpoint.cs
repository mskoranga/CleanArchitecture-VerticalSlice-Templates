using CleanArchitecture.VerticalSlice.Application.Abstractions;
using CleanArchitecture.VerticalSlice.Application.Constants;
using CleanArchitecture.VerticalSlice.Application.Extensions;
using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;
using CleanArchitecture.VerticalSlice.Domain.Abstractions;

namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemaMessage;

internal sealed class ProduceOrderEventEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("kafka/produce/order-event", async (
            IHandler<ProduceSchemaMessageRequest<OrderCreatedEvent>, Result<ProduceSchemaMessageResponse>> handler,
            OrderCreatedEvent orderEvent,
            string topic,
            CancellationToken cancellationToken) =>
        {
            var request = new ProduceSchemaMessageRequest<OrderCreatedEvent>(
                Topic: topic,
                Key: orderEvent.OrderId,
                Schema: orderEvent.Schema,
                SchemaVersion: "1.0",
                Payload: orderEvent);

            var result = await handler.HandleAsync(request, cancellationToken);
            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Kafka)
        .WithName("ProduceOrderEvent")
        .WithSummary("Produce an OrderCreatedEvent with schema validation")
        .WithDescription("Sends a strongly-typed order event to Kafka with schema validation")
        .Produces<ProduceSchemaMessageResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}

internal sealed class ProduceUserEventEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("kafka/produce/user-event", async (
            IHandler<ProduceSchemaMessageRequest<UserRegisteredEvent>, Result<ProduceSchemaMessageResponse>> handler,
            UserRegisteredEvent userEvent,
            string topic,
            CancellationToken cancellationToken) =>
        {
            var request = new ProduceSchemaMessageRequest<UserRegisteredEvent>(
                Topic: topic,
                Key: userEvent.UserId,
                Schema: userEvent.Schema,
                SchemaVersion: "1.0",
                Payload: userEvent);

            var result = await handler.HandleAsync(request, cancellationToken);
            return result.Match(
                onSuccess: response => Results.Ok(response),
                onFailure: error => Results.BadRequest(error));
        })
        .WithTags(ApiTags.Kafka)
        .WithName("ProduceUserEvent")
        .WithSummary("Produce a UserRegisteredEvent with schema validation")
        .WithDescription("Sends a strongly-typed user registration event to Kafka with schema validation")
        .Produces<ProduceSchemaMessageResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest);
    }
}
