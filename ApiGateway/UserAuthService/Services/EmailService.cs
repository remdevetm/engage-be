using SendGrid.Helpers.Mail;
using SendGrid;
using UserAuthService.Templates;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Services
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
                var msg = MailHelper.CreateSingleEmail(from_email, new EmailAddress(recipientEmail, "Njinu Kimani"),
                    subject, body, body);
                var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string GetEmailBody(EmailType emailType, string content = "")
        {
            var html = new EmailTemplate().GetHTMLTemplate(emailType);
            switch (emailType)
            {
                case EmailType.Welcome:
                    return html;
                    break;
                case EmailType.OTP:
                    html = html.Replace("[Your OTP Here]", content);
                    return html;
                    break;
                case EmailType.Notification:
                    html = html.Replace("[Content]", content);
                    return html;
                    break;
                case EmailType.LoginDetail:
                    var parts = content.Split(',');
                    if (parts.Length == 4)
                    {
                        html = html.Replace("[Name]", parts[0])
                                   .Replace("[UserEmail]", parts[1])
                                   .Replace("[UserPassword]", parts[2])
                                   .Replace("[UserRole]", parts[3]);
                    }
                    return html;
                    break;
                default:
                    return html;
                    break;
            }
        }

        public async Task<Response> SendEmailSendgrid()
        {
            var subject = "Sending with Twilio SendGrid is Fun";
            var to_email = new EmailAddress("njinukimani@gmail.com", "Njinu Kimani");
            var plainTextContent = "and easy to do anywhere, even with C#";
            var htmlContent = "<strong>and easy to do anywhere, even with C#</strong>";
            var msg = MailHelper.CreateSingleEmail(from_email, to_email, subject, plainTextContent, htmlContent);
            var response = await client.SendEmailAsync(msg).ConfigureAwait(false);
            return response;
        }
    }
}
