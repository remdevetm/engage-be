namespace Comms.Application.Features.Message.Commands.SendEmailMessage;

public class SendEmailCommand : ICommand<CreateMessageResult>
{
    public MessageDto Message { get; init; }
    public SendEmailCommand(MessageDto message)
    {
        Message = message;
    }
}
