namespace Comms.Application.Features.Message.Commands.CreateMessage;

public class CreateMessageHandler(IApplicationDbContext dbContext)
    : ICommandHandler<CreateMessageCommand, CreateMessageResult>
{
    public async Task<CreateMessageResult> Handle(CreateMessageCommand command, CancellationToken cancellationToken)
    {

        var message = CreateNewMessage(command.Message);

        dbContext.Messages.Add(message);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new CreateMessageResult(message.Id);
    }

    private Domain.Models.Message CreateNewMessage(MessageDto messageDto)
    {
        var newMessage = Domain.Models.Message.Create(
                id: Guid.NewGuid().ToString(),
                clientId: messageDto.ClientId,
                agentId: messageDto.AgentId,
                replyToMessageId: messageDto.ReplyToMessageId,
                text: messageDto.Text,
                channel: Channel.None,
                direction: Direction.None,
                from: "System",
                subject: messageDto.Subject,
                to: messageDto.To
                );
        return newMessage;
    }
}
