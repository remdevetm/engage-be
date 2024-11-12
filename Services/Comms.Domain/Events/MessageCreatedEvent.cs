namespace Comms.Domain.Events;
public record MessageCreatedEvent(Message message) : IDomainEvent;