
namespace Comms.Application.Messages.Commands.ReceiveInboundEmail
{
    public class RecieveInboundEmailCommand :ICommand<CreateMessageResult>
    {
        public SendGridInboundEmailDto InboundEmail { get; init; }
        public RecieveInboundEmailCommand(SendGridInboundEmailDto inboundEmail)
        {
            InboundEmail = inboundEmail;
        }
    }
}
