namespace Comms.Application.Features.Lead.Queries.GetLeads;

public record GetLeadsQuery()
: IQuery<GetLeadsQueryResult>;

public record GetLeadsQueryResult(IEnumerable<LeadDto> Leads);
