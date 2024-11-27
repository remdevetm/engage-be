namespace Comms.Domain.ValueObjects;
public record MessageId
{
    public Guid Value { get; }
    private MessageId(Guid value) => Value = value;
    public static MessageId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new DomainException("MessageId cannot be empty.");
        }

        return new MessageId(value);
    }
}