using FluentValidation;

namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ProduceSchemalessMessage;

public sealed class ProduceSchemalessMessageValidator : AbstractValidator<ProduceSchemalessMessageRequest>
{
    public ProduceSchemalessMessageValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required")
            .MaximumLength(249).WithMessage("Topic must not exceed 249 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Topic must contain only alphanumeric characters, dots, underscores, and hyphens");

        RuleFor(x => x.Payload)
            .NotNull().WithMessage("Payload is required")
            .Must(p => p != null && p.Count > 0).WithMessage("Payload must contain at least one key-value pair");

        RuleFor(x => x.Key)
            .MaximumLength(500).WithMessage("Key must not exceed 500 characters")
            .When(x => !string.IsNullOrEmpty(x.Key));
    }
}
