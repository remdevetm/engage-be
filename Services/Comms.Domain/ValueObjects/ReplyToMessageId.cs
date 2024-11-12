namespace Comms.Domain.ValueObjects;
public record ReplyToMessageId
{
    public Guid Value { get; }
    private ReplyToMessageId(Guid value) => Value = value;
    public static ReplyToMessageId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new DomainException("ReplyToMessageId cannot be empty.");
        }

        return new ReplyToMessageId(value);
    }
}