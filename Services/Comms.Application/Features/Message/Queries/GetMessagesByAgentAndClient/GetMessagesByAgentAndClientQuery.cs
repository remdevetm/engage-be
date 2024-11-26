namespace Comms.Application.Features.Message.Queries.GetMessagesByAgentAndClient
{
    public record GetMessagesByAgentAndClientQuery(string agentId, string clientId)
    : IQuery<GetMessagesByAgentAndClientResult>;

    public record GetMessagesByAgentAndClientResult(IEnumerable<MessageDto> Messages);
}
