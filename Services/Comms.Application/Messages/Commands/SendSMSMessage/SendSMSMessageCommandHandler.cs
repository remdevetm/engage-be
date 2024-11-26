namespace Comms.Application.Messages.Commands.SendSMSMessage
{
    public class SendSMSMessageCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<SendSMSMessageCommand, CreateMessageResult>
    {
        public async Task<CreateMessageResult> Handle(SendSMSMessageCommand command, CancellationToken cancellationToken)
        {
            var message = CreateNewMessage(command.Message);

            dbContext.Messages.Add(message);
            await dbContext.SaveChangesAsync(cancellationToken);

            return new CreateMessageResult(message.Id);
        }

        private Message CreateNewMessage(MessageDto messageDto)
        {
            var message = Message.Create(
                               id: Guid.NewGuid().ToString(),
                               clientId: messageDto.ClientId,
                               agentId: messageDto.AgentId,
                               replyToMessageId: messageDto.ReplyToMessageId,
                               text: messageDto.Text,
                               channel: Channel.SMS,
                               direction: Direction.Outbound,
                               from: "System",
                               subject: messageDto.Subject,
                               to: messageDto.To);
            return message;
        }
    }
}
