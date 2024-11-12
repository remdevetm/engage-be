namespace Comms.Application.Data;
public interface IApplicationDbContext
{
    DbSet<Message> Messages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
