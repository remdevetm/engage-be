

namespace Comms.Application.Messages.Commands.MarkMessageAsRead
{
    public record MarkMessageAsReadCommand(string messageId) : ICommand<string>;
}
