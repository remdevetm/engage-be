namespace Comms.Application.Dtos;
public record MessageDto(
    string Id,
    string ClientId,
    string AgentId,
    string ReplyToMessageId,
    string Text,
    string Subject,
    string To);