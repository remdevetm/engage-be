
namespace Comms.Application.Messages.Queries.GetMessagesByAgent;
public record GetMessagesByAgentQuery(string agentId)
: IQuery<GetMessagesByAgentResult>;

public record GetMessagesByAgentResult(IEnumerable<MessageDto> Messages);