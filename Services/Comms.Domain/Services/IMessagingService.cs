namespace Comms.Domain.Services
{
    public interface IMessagingService
    {
        Task SendEmail(string recipientEmail, string subject, string body);
        Task SendSms(string recipientPhoneNumber, string messageBody);
        Task SendWhatsApp(string recipientPhoneNumber, string messageBody);
    }
}
