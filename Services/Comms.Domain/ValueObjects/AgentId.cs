namespace Comms.Domain.ValueObjects;
public record AgentId
{
    public Guid Value { get; }
    private AgentId(Guid value) => Value = value;
    public static AgentId Of(Guid value)
    {
        ArgumentNullException.ThrowIfNull(value);
        if (value == Guid.Empty)
        {
            throw new DomainException("AgentId cannot be empty.");
        }

        return new AgentId(value);
    }
}