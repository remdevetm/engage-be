namespace Comms.Application.Features.Message.Commands.CreateMessage
{
    public class CreateMessageCommand : ICommand<CreateMessageResult>
    {
        public MessageDto Message { get; init; }
        public CreateMessageCommand(MessageDto message)
        {
            Message = message;
        }
    }

    public record CreateMessageResult(string Id);
}
