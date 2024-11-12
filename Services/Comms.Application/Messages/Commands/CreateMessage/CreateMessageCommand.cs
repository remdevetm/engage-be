namespace Comms.Application.Messages.Commands.CreateMessage;
public class CreateMessageCommand : ICommand<CreateMessageResult>
{
    public MessageDto Message { get; init; }
    public CreateMessageCommand(MessageDto message)
    {
        Message = message;
    }
}

public record CreateMessageResult(string Id);

public class CreateMessageCommandValidator : AbstractValidator<CreateMessageCommand>
{
    public CreateMessageCommandValidator()
    {
        RuleFor(x => x.Message.ClientId).NotNull().WithMessage("ClientId is required");
        RuleFor(x => x.Message.AgentId).NotNull().WithMessage("AgentId is required");
        RuleFor(x => x.Message.Text).NotNull().WithMessage("Message is required");
        RuleFor(x => x.Message.Channel).NotNull().WithMessage("Channel is required");
        RuleFor(x => x.Message.Direction).NotNull().WithMessage("Direction is required");
        RuleFor(x => x.Message.From).NotNull().WithMessage("From is required");
        RuleFor(x => x.Message.To).NotNull().WithMessage("To is required");
    }
}