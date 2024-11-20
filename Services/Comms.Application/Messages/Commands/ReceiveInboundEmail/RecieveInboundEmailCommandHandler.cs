namespace Comms.Application.Messages.Commands.ReceiveInboundEmail
{
    public class RecieveInboundEmailCommandHandler(IApplicationDbContext dbContext) :ICommandHandler<RecieveInboundEmailCommand, CreateMessageResult>
    {
        public async Task<CreateMessageResult> Handle(RecieveInboundEmailCommand command, CancellationToken cancellationToken)
        {
            var oldMessage = await dbContext.Messages
                                    .Where(m => m.From == command.InboundEmail.From && m.To == command.InboundEmail.To)
                                    .OrderByDescending(m => m.CreatedAt)
                                    .FirstOrDefaultAsync(cancellationToken);

            var message = CreateNewMessage(command.InboundEmail,oldMessage);
         
            dbContext.Messages.Add(message);
            await dbContext.SaveChangesAsync(cancellationToken);


            return new CreateMessageResult(message.Id);
        }

        private Message CreateNewMessage(SendGridInboundEmailDto inboundEmail, Message oldMessage)
        {
            var message = Message.Create(
                                              id: Guid.NewGuid().ToString(),
                                              clientId: oldMessage.ClientId,
                                              agentId: oldMessage.AgentId,
                                              replyToMessageId: oldMessage.Id,
                                              text: inboundEmail.Text,
                                              channel: Channel.Email,
                                              direction: Direction.Inbound,
                                              from: inboundEmail.From,
                                              subject: inboundEmail.Subject,
                                              to: inboundEmail.To,
                                               status: Status.Sent);
            return message;
        }
    }
}
