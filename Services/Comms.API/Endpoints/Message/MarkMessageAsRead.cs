namespace Comms.API.Endpoints.Message
{
    public class MarkMessageAsRead : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPatch("/message/agent/markasread/{messageId}", async (string messageId, ISender sender) =>
            {
                var command = new MarkMessageAsReadCommand(messageId);

                var result = await sender.Send(command);

                return Results.Ok(result);
            })
 .WithName("MarkAsRead")
 .Produces<CreateMessageResponse>(StatusCodes.Status200OK)
 .ProducesProblem(StatusCodes.Status400BadRequest)
 .WithSummary("mark email as read")
 .WithDescription("Handles marking email as read");
        }
    }
}
