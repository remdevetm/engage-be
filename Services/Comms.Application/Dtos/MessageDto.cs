namespace Comms.Application.Dtos;
public record MessageDto(
    string Id,
    string ClientId,
    string AgentId,
    string ReplyToMessageId,
    string Text,
    Channel Channel,
    Direction Direction,
    string From,
    string To);