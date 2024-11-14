namespace Comms.API.Endpoints.Message
{
    public class SendEmailMessage:ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/message/agent/sendemail", async (CreateMessageRequest request, ISender sender) =>
            {
                var command = request.Adapt<SendEmailCommand>();

                var result = await sender.Send(command);

                var response = result.Adapt<CreateMessageResponse>();

                return Results.Created($"/message/{response.Id}", response);
            })
            .WithName("AgentSendEmailToClient")
            .Produces<CreateMessageResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Send Email to Client")
            .WithDescription("Handles email sending by an agent to a client");
        }
    }
}
