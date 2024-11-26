using Comms.Application.Contracts.Infrastructure.Persistence;

namespace Comms.Application.Features.Lead.Queries.GetLeads
{
    public class GetLeadsQueryHandler()
    : IQueryHandler<GetLeadsQuery, GetLeadsQueryResult>
    {
        public async Task<GetLeadsQueryResult> Handle(GetLeadsQuery query, CancellationToken cancellationToken)
        {
            return new GetLeadsQueryResult(new List<LeadDto>());
        }
    }
}
