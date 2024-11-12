namespace Comms.Application.Messages.EventHandlers.Domain;
public class MessageCreatedEventHandler
    (IPublishEndpoint publishEndpoint, IFeatureManager featureManager, ILogger<MessageCreatedEventHandler> logger, IEmailService emailService)
    : INotificationHandler<MessageCreatedEvent>
{
    public async Task Handle(MessageCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        logger.LogInformation("Domain Event handled: {DomainEvent}", domainEvent.GetType().Name);

        if (await featureManager.IsEnabledAsync("MessageFullfilment"))
        {
            var messageCreatedIntegrationEvent = domainEvent.message.ToMessageDto();
            if(messageCreatedIntegrationEvent.Channel == Channel.Email && messageCreatedIntegrationEvent.Direction == Direction.Outbound)
                await emailService.SendEmail(messageCreatedIntegrationEvent.To, "Comms", messageCreatedIntegrationEvent.Text);
            //await publishEndpoint.Publish(messageCreatedIntegrationEvent, cancellationToken);
        }
    }
}
