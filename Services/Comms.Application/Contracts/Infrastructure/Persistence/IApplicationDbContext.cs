namespace Comms.Application.Contracts.Infrastructure.Persistence;

public interface IApplicationDbContext
{
    DbSet<Message> Messages { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}
