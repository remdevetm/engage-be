namespace Comms.Application.Exceptions
{
    public class MessageNotFoundException : NotFoundException
    {
        public MessageNotFoundException(Guid id) : base("Message", id)
        {
        }
    }

}
