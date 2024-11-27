using Comms.Application.Features.Message.Commands.CreateMessage;

namespace Comms.Application.Features.Message.Commands.SendWhatsappMessage
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
