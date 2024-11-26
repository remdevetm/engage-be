namespace Comms.Application.Features.Message.Queries.GetMessagesByAgentAndClient
{
    public class GetMessagesByAgentClientHandler(IApplicationDbContext dbContext)
        : IQueryHandler<GetMessagesByAgentAndClientQuery, GetMessagesByAgentAndClientResult>
    {
        public async Task<GetMessagesByAgentAndClientResult> Handle(GetMessagesByAgentAndClientQuery query, CancellationToken cancellationToken)
        {

            var messages = await dbContext.Messages
                            .AsNoTracking()
                            .Where(m => m.AgentId == query.agentId && m.ClientId == query.clientId)
                            .OrderByDescending(m => m.CreatedAt)
                            .ToListAsync(cancellationToken);

            return new GetMessagesByAgentAndClientResult(messages.ToMessageDtoList());
        }
    }
}
