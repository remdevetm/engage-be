namespace Comms.Application.Messages.Queries.GetMessagesByAgent;

public class GetMessagesByAgentHandler(IApplicationDbContext dbContext)
    : IQueryHandler<GetMessagesByAgentQuery, GetMessagesByAgentResult>
{
    public async Task<GetMessagesByAgentResult> Handle(GetMessagesByAgentQuery query, CancellationToken cancellationToken)
    {

        var messages = await dbContext.Messages
                        .AsNoTracking()
                        .Where(m => m.AgentId == query.agentId)
                        .OrderByDescending(m => m.CreatedAt)
                        .ToListAsync(cancellationToken);

        return new GetMessagesByAgentResult(messages.ToMessageDtoList());
    }
}