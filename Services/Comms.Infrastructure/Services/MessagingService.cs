using Twilio.Types;

namespace Comms.Infrastructure.Services
{
    public class MessagingService :IMessagingService
    {
        // Email Configuration
        private readonly string _emailApiKey;
        private readonly EmailAddress _fromEmail;
        private readonly SendGridClient _emailClient;

        // SMS Configuration
        private readonly string _twilioAccountSid;
        private readonly string _twilioAuthToken;
        private readonly string _twilioPhoneNumber;
        private readonly IConfiguration _configuration;
        public MessagingService(IConfiguration configuration)
        {
            // Initialize Email Service
            _emailApiKey = configuration["Messaging:Providers:Email:ApiKey"] ?? throw new ArgumentNullException("Email API Key is missing");
            var senderEmail = configuration["Messaging:Providers:Email:SenderEmail"] ?? "default@domain.com";
            var senderName = configuration["Messaging:Providers:Email:SenderName"] ?? "Default Sender";
            _emailClient = new SendGridClient(_emailApiKey);
            _fromEmail = new EmailAddress(senderEmail, senderName);


            // Initialize SMS Service
            _twilioAccountSid = configuration["Messaging:Providers:Sms:AccountSid"] ?? throw new ArgumentNullException("Twilio Account SID is missing");
            _twilioAuthToken = configuration["Messaging:Providers:Sms:AuthToken"] ?? throw new ArgumentNullException("Twilio Auth Token is missing");
            _twilioPhoneNumber = configuration["Messaging:Providers:Sms:FromNumber"] ?? throw new ArgumentNullException("Twilio Phone Number is missing");
            _configuration = configuration;
        }

        public async Task SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                var toEmail = new EmailAddress(recipientEmail);
                var msg = MailHelper.CreateSingleEmail(_fromEmail, toEmail, subject, body, body);

                var response = await _emailClient.SendEmailAsync(msg).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Failed to send email: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while sending email: {ex.Message}", ex);
            }
        }

        public async Task SendSms(string recipientPhoneNumber, string messageBody)
        {
            try
            {
                TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);
                var message = await MessageResource.CreateAsync(
                    to: new Twilio.Types.PhoneNumber(recipientPhoneNumber),
                    from: new Twilio.Types.PhoneNumber(_twilioPhoneNumber),
                    body: messageBody
                );
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while sending SMS: {ex.Message}", ex);
            }
        }

        public async Task SendWhatsApp(string recipientPhoneNumber, string messageBody)
        {
            try
            {
                TwilioClient.Init(_configuration["Messaging:Providers:WhatsApp:AccountSid"], _configuration["Messaging:Providers:WhatsApp:AuthToken"]);
                var message = await MessageResource.CreateAsync(
                    to: new Twilio.Types.PhoneNumber($"whatsapp:{recipientPhoneNumber}"),
                    from: new Twilio.Types.PhoneNumber($"whatsapp:{_configuration["Messaging:Providers:WhatsApp:FromNumber"]}"),
                    body: messageBody
                );

                if (message.Status == MessageResource.StatusEnum.Failed || message.Status == MessageResource.StatusEnum.Undelivered)
                {
                    throw new Exception($"Failed to send WhatsApp message: {message.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while sending WhatsApp message: {ex.Message}", ex);
            }
        }
    }

}
