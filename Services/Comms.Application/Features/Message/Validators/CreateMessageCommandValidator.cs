namespace Comms.Application.Features.Message.Validators
{
    public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
    {
        public CreateMessageCommandValidator()
        {
            RuleFor(x => x.Message.ClientId).NotNull().WithMessage("ClientId is required");
            RuleFor(x => x.Message.AgentId).NotNull().WithMessage("AgentId is required");
            RuleFor(x => x.Message.Text).NotNull().WithMessage("Message is required");
            RuleFor(x => x.Message.Subject).NotNull().WithMessage("Subject is required");
            RuleFor(x => x.Message.To).NotNull().WithMessage("To is required");
        }
    }
}
