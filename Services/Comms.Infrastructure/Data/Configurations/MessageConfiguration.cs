namespace Comms.Infrastructure.Data.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.Property(m => m.Id);

        builder.Property(m => m.AgentId);

        builder.Property(m => m.ClientId);

        builder.Property(m => m.ReplyToMessageId);

        builder.Property(m => m.Text);

        builder.Property(m => m.Channel)
                .HasConversion(
                 m => m.ToString(),
                dbStatus => (Channel)Enum.Parse(typeof(Channel), dbStatus));

        builder.Property(m => m.Direction)
                .HasConversion(
                m => m.ToString(),
                dbStatus => (Direction)Enum.Parse(typeof(Direction), dbStatus));

        builder.Property(m => m.Status)
               .HasConversion(
               m => m.ToString(),
               dbStatus => (Status)Enum.Parse(typeof(Status), dbStatus));

        builder.Property(m => m.From);

        builder.Property(m => m.To);
    }
}
