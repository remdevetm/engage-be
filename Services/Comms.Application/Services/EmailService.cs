using Comms.Application.Services.Interfaces;
using Comms.Domain.ValueObjects;
using SendGrid.Helpers.Mail;
using SendGrid;

namespace Comms.Application.Services
{
    public class EmailService : IEmailService
    {
        private readonly string? _senderEmail;
        private readonly string apiKey = "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";
        private readonly SendGridClient client;
        private readonly EmailAddress from_email;
        public EmailService()
        {
            client = new SendGridClient(apiKey);
            from_email = new EmailAddress("dev1@webparam.org", "Dev Bot");
        }
        public async Task SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                var msg = MailHelper.CreateSingleEmail(from_email, new EmailAddress(recipientEmail, ""),
                    subject, body, body);
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
