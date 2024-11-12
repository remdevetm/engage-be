namespace Comms.Application.Messages.Queries.GetMessagesByAgentAndClient;
public record GetMessagesByAgentAndClientQuery(string agentId, string clientId)
: IQuery<GetMessagesByAgentAndClientResult>;

public record GetMessagesByAgentAndClientResult(IEnumerable<MessageDto> Messages);