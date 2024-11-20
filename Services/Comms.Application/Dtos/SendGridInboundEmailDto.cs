namespace Comms.Application.Dtos
{
    public class SendGridInboundEmailDto
    {
        public string SPF { get; set; }
        public string Attachments { get; set; }
        public string Charsets { get; set; }
        public string Dkim { get; set; }
        public string Envelope { get; set; }
        public string From { get; set; }
        public string Headers { get; set; }
        public string Html { get; set; }
        public string SenderIp { get; set; }
        public string Subject { get; set; }
        public string Text { get; set; }
        public string To { get; set; }
    }
}
