namespace UserAuthService.Services.Interfaces
{
    public interface IHashingService
    {
        string Hash(string password, out string salt);
        bool Verify(string password, string storedHashWithSalt);
    }
}
