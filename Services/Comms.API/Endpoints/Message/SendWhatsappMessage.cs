namespace Comms.API.Endpoints.Message
{
    public class SendWhatsappMessage:ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/message/agent/sendwhatsapp", async (CreateMessageRequest request, ISender sender) =>
            {
                var command = request.Adapt<SendWhatsappMessageCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreateMessageResponse>();

                return Results.Created($"/message/{response.Id}", response);
            })
            .WithName("AgentSendWhatsappMessageToClient")
            .Produces<CreateMessageResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Send SMS to Client")
            .WithDescription("Handles sms sending by an agent to a client");
        }
    }
}
