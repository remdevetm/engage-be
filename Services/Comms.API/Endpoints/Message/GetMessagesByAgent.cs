using Comms.Application.Messages.Queries.GetMessagesByAgent;

namespace Comms.API.Endpoints.Message;

public record GetMessagesByAgentResponse(IEnumerable<MessageDto> Messages);

public class GetMessagesByAgent : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/messages/agent/{agentId}", async (string agentId, ISender sender) =>
        {
            var result = await sender.Send(new GetMessagesByAgentQuery(agentId));

            var response = result.Adapt<GetMessagesByAgentResponse>();

            return Results.Ok(response);
        })
        .WithName("GetMessagesByAgent")
        .Produces<GetMessagesByAgentResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Messages By Agent")
        .WithDescription("Get Message By Agent");
    }
}
