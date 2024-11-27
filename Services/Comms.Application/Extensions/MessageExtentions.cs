
namespace Comms.Application.Extensions
{
    public static class MessageExtentions
    {
        public static IEnumerable<MessageDto> ToMessageDtoList(this IEnumerable<Message> messages)
        {
            return messages.Select(message => new MessageDto(
                Id: message.Id,
                ClientId: message.ClientId,
                AgentId: message.AgentId,
                ReplyToMessageId: message.ReplyToMessageId,
                Text: message.Text,
                Channel: message.Channel,
                Direction: message.Direction,
                From: message.From,
                To: message.To
            ));
        }

        public static MessageDto ToMessageDto(this Message message)
        {
            return DtoFromMessage(message);
        }

        private static MessageDto DtoFromMessage(Message message)
        {
            return new MessageDto(
                Id: message.Id,
                ClientId: message.ClientId,
                AgentId: message.AgentId,
                ReplyToMessageId: message.ReplyToMessageId,
                Text: message.Text,
                Channel: message.Channel,
                Direction: message.Direction,
                From: message.From,
                To: message.To
             );
        }
    }
}
