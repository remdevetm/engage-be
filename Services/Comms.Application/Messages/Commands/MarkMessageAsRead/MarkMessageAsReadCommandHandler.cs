
namespace Comms.Application.Messages.Commands.MarkMessageAsRead
{
    public class MarkMessageAsReadCommandHandler(IApplicationDbContext dbContext) : ICommandHandler<MarkMessageAsReadCommand, string>
    {

        public async Task<string> Handle(MarkMessageAsReadCommand request, CancellationToken cancellationToken)
        {
            var message = await dbContext.Messages
                                   .Where(m => m.Id == request.messageId)
                                   .OrderByDescending(m => m.CreatedAt)
                                   .FirstOrDefaultAsync(cancellationToken);

            message.MarkAsRead();
            dbContext.Messages.Update(message);
            await dbContext.SaveChangesAsync(cancellationToken);
            return message.Id;

        }
    }
}
