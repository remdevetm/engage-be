namespace Comms.API.Endpoints.Message;
public record CreateMessageRequest(MessageDto Message);
public record CreateMessageResponse(Guid Id);
public class CreateMessage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/message", async (CreateMessageRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateMessageCommand>();

            var result = await sender.Send(command);

            var response = result.Adapt<CreateMessageResponse>();

            return Results.Created($"/message/{response.Id}", response);
        })
        .WithName("CreateMessage")
        .Produces<CreateMessageResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Create Message")
        .WithDescription("Create Message");
    }
}
