using UserAuthService.Templates;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace UserAuthService.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmail(string recipientEmail, string subject, string body);
        string GetEmailBody(EmailType emailType, string content = "");
        Task<Response> SendEmailSendgrid();
    }
}
