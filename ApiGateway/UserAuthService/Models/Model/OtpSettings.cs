namespace UserAuthService.Models.Model
{
    public class OtpSettings
    {
        public int ExpiryMinutes { get; set; }
        public int MaxAttempts { get; set; }
        public int LockoutMinutes { get; set; }
    }
}
