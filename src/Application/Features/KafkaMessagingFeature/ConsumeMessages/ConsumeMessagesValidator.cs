using FluentValidation;

namespace CleanArchitecture.VerticalSlice.Application.Features.KafkaMessagingFeature.ConsumeMessages;

public sealed class ConsumeMessagesValidator : AbstractValidator<ConsumeMessagesRequest>
{
    public ConsumeMessagesValidator()
    {
        RuleFor(x => x.Topic)
            .NotEmpty().WithMessage("Topic is required")
            .MaximumLength(249).WithMessage("Topic must not exceed 249 characters")
            .Matches(@"^[a-zA-Z0-9._-]+$").WithMessage("Topic must contain only alphanumeric characters, dots, underscores, and hyphens");

        RuleFor(x => x.ConsumerGroupId)
            .NotEmpty().WithMessage("ConsumerGroupId is required")
            .MaximumLength(255).WithMessage("ConsumerGroupId must not exceed 255 characters");

        RuleFor(x => x.MaxMessages)
            .GreaterThan(0).WithMessage("MaxMessages must be greater than 0")
            .LessThanOrEqualTo(1000).WithMessage("MaxMessages must not exceed 1000");
    }
}
