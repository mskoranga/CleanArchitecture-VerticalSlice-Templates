using CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.Models;
using FluentValidation;

namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemaMessage;

public sealed class ProduceOrderEventValidator : AbstractValidator<ProduceSchemaMessageRequest<OrderCreatedEvent>>
{
    public ProduceOrderEventValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required")
            .MaximumLength(249).WithMessage("Topic must not exceed 249 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Topic must contain only alphanumeric characters, dots, underscores, and hyphens");

        RuleFor(x => x.Schema)
            .NotEmpty().WithMessage("Schema is required");

        RuleFor(x => x.SchemaVersion)
            .NotEmpty().WithMessage("Schema version is required")
            .Matches(@"^\d+\.\d+$").WithMessage("Schema version must be in format 'major.minor' (e.g., '1.0')");

        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Payload is required")
            .SetValidator(new OrderCreatedEventValidator());
    }
}

public sealed class OrderCreatedEventValidator : AbstractValidator<OrderCreatedEvent>
{
    public OrderCreatedEventValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId is required");

        RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("TotalAmount must be greater than 0");

        RuleFor(x => x.OrderDate)
            .NotEmpty().WithMessage("OrderDate is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("OrderDate cannot be in the future");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => new[] { "Pending", "Processing", "Completed", "Cancelled" }.Contains(status))
            .WithMessage("Status must be one of: Pending, Processing, Completed, Cancelled");
    }
}

public sealed class ProduceUserEventValidator : AbstractValidator<ProduceSchemaMessageRequest<UserRegisteredEvent>>
{
    public ProduceUserEventValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required")
            .MaximumLength(249).WithMessage("Topic must not exceed 249 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Topic must contain only alphanumeric characters, dots, underscores, and hyphens");

        RuleFor(x => x.Schema)
            .NotEmpty().WithMessage("Schema is required");

        RuleFor(x => x.SchemaVersion)
            .NotEmpty().WithMessage("Schema version is required")
            .Matches(@"^\d+\.\d+$").WithMessage("Schema version must be in format 'major.minor' (e.g., '1.0')");

        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Payload is required")
            .SetValidator(new UserRegisteredEventValidator());
    }
}

public sealed class UserRegisteredEventValidator : AbstractValidator<UserRegisteredEvent>
{
    public UserRegisteredEventValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Email must be a valid email address");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.RegisteredAt)
            .NotEmpty().WithMessage("RegisteredAt is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("RegisteredAt cannot be in the future");
    }
}
