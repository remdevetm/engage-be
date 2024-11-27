namespace Comms.Application.Features.Message.EventHandlers.Domain
{
    public class MessageCreatedEventHandler
        (IPublishEndpoint publishEndpoint, IFeatureManager featureManager, ILogger<MessageCreatedEventHandler> logger, IMessageService messagingService)
        : INotificationHandler<MessageCreatedEvent>
    {
        public async Task Handle(MessageCreatedEvent domainEvent, CancellationToken cancellationToken)
        {
            logger.LogInformation("Domain Event handled: {DomainEvent}", domainEvent.GetType().Name);

            if (await featureManager.IsEnabledAsync("MessageFullfilment"))
            {
                var messageCreatedIntegrationEvent = domainEvent.message;
                if (messageCreatedIntegrationEvent.Channel == Channel.Email && messageCreatedIntegrationEvent.Direction == Direction.Outbound)
                    await messagingService.SendEmail(messageCreatedIntegrationEvent.To, messageCreatedIntegrationEvent.Subject, messageCreatedIntegrationEvent.Text);
                else if (messageCreatedIntegrationEvent.Channel == Channel.SMS && messageCreatedIntegrationEvent.Direction == Direction.Outbound)
                    await messagingService.SendSms(messageCreatedIntegrationEvent.To, messageCreatedIntegrationEvent.Text);
                else if (messageCreatedIntegrationEvent.Channel == Channel.Whatsapp && messageCreatedIntegrationEvent.Direction == Direction.Outbound)
                    await messagingService.SendWhatsApp(messageCreatedIntegrationEvent.To, messageCreatedIntegrationEvent.Text);
                //await publishEndpoint.Publish(messageCreatedIntegrationEvent, cancellationToken);
            }
        }
    }
}
