
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
                Subject: message.Subject,
                Text: message.Text,
                To: message.To,
                From: message.From
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
                Subject: message.Subject,
                To: message.To,
                From: message.From
             );
        }
    }
}
