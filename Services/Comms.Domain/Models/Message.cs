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
    public string From { get; private set; } = default!;
    public string To { get; private set; } = default!;


    public static Message Create(string id, string clientId, string agentId, string replyToMessageId, string text, Channel channel, Direction direction,
        string from, string to, string subject)
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
            Subject = subject
        };

        message.AddDomainEvent(new MessageCreatedEvent(message));

        return message;
    }
}


