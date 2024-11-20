namespace Comms.Domain.Models;

public class Message : Aggregate<string>
{
    public string ClientId { get; private set; } = default!;
    public string AgentId { get; private set; } = default!;
    public string ReplyToMessageId { get; private set; } = default!;
    public string Subject { get; private set; } = default!;
    public string Text { get; private set; } = default!;
    public Channel Channel { get; private set; } = Channel.None;
    public Direction Direction { get; private set; } = Direction.None;
    public Status Status { get; private set; } = Status.New;
    public string From { get; private set; } = default!;
    public string To { get; private set; } = default!;


    public static Message Create(string id, string clientId, string agentId, string replyToMessageId, string text, Channel channel, Direction direction,
        string from, string to, string subject, Status status)
    {
        var message = new Message
        {
            Id = id,
            ClientId = clientId,
            AgentId = agentId,
            ReplyToMessageId = replyToMessageId,
            Text = text,
            Channel = channel,
            Direction = direction,
            From = from,
            To = to,
            Subject = subject,
            Status = status
            
        };

        message.AddDomainEvent(new MessageCreatedEvent(message));

        return message;
    }

    public void MarkAsRead()
    {
        if (Status == Status.Opened)
        {
            return;
        }

        Status = Status.Opened;
        LastModified = DateTime.UtcNow;
    }
}


