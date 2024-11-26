namespace Comms.Application.Features.Message.Queries.GetMessagesByAgent;

public record GetMessagesByAgentQuery(string agentId)
: IQuery<GetMessagesByAgentResult>;

public record GetMessagesByAgentResult(IEnumerable<MessageDto> Messages);
