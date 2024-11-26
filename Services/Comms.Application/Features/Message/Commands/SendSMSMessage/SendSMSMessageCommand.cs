namespace Comms.Application.Features.Message.Commands.SendSMSMessage;

public class SendSMSMessageCommand : ICommand<CreateMessageResult>
{
    public MessageDto Message { get; init; }
    public SendSMSMessageCommand(MessageDto message)
    {
        Message = message;
    }
}
