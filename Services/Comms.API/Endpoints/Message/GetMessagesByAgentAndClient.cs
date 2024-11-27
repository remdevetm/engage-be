namespace Comms.API.Endpoints.Message;

public record GetMessagesByAgentAndClientResponse(IEnumerable<MessageDto> Messages);

public class GetMessagesByAgentAndClient : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/messages/agentandclient/{agentId}", async (string agentId, string clientId, ISender sender) =>
        {
            var result = await sender.Send(new GetMessagesByAgentAndClientQuery(agentId, clientId));

            var response = result.Adapt<GetMessagesByAgentAndClientResponse>();

            return Results.Ok(response);
        })
        .WithName("GetMessagesByAgentAndClient")
        .Produces<GetMessagesByAgentAndClientResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Messages By Agent And Client")
        .WithDescription("Get Message By Agent And Client");
    }
}
