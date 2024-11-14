

namespace Comms.Application.Messages.Commands.SendWhatsappMessage
{
    public class SendWhatsappMessageCommand : ICommand<CreateMessageResult>
    {
        public MessageDto Message { get; init; }
        public SendWhatsappMessageCommand(MessageDto message)
        {
            Message = message;
        }
    }
}
