
namespace Comms.Application.Services.Interfaces;
public interface IEmailService
{
    Task SendEmail(string recipientEmail, string subject, string body);
}
