namespace Comms.Application.Contracts.Infrastructure.Services.Message
{
    public interface IMessageService
    {
        Task SendEmail(string recipientEmail, string subject, string body);
        Task SendSms(string recipientPhoneNumber, string messageBody);
        Task SendWhatsApp(string recipientPhoneNumber, string messageBody);
    }
}
