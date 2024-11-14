namespace Comms.API.Endpoints.Message
{
    public class SendSMSMessage:ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/message/agent/sendsms", async (CreateMessageRequest request, ISender sender) =>
            {
                var command = request.Adapt<SendSMSMessageCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreateMessageResponse>();

                return Results.Created($"/message/{response.Id}", response);
            })
            .WithName("AgentSendSMSMessageToClient")
            .Produces<CreateMessageResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Send SMS to Client")
            .WithDescription("Handles sms sending by an agent to a client");
        }
    }
}
