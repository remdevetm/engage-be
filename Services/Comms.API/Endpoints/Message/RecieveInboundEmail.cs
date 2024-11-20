using Comms.Application.Messages.Commands.ReceiveInboundEmail;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Comms.API.Endpoints.Message
{
    public class RecieveInboundEmail:ICarterModule
    {
        public record RecieveMessageRequest(SendGridInboundEmailDto Body);
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapPost("/message/agent/receivemail", async (SendGridInboundEmailDto request, ISender sender) =>
            {
            // Extract the email using Regex
                var emailPattern = @"<([^>]+)>";
                 var match = Regex.Match(request.From, emailPattern);
                if (match.Success)
                {
                    var email = match.Groups[1].Value;
                    request.From = email;
                }
                var command = new RecieveInboundEmailCommand(request);
               
                var result = await sender.Send(command);

                var response = result.Adapt<CreateMessageResponse>();

                return Results.Created($"/message/{response.Id}", response);
            })
            .WithName("AgentRecieveEmailToClient")
            .Produces<CreateMessageResponse>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Recieve Email to Client")
            .WithDescription("Handles email recieving by an agent from a client");
        }
    }
}
