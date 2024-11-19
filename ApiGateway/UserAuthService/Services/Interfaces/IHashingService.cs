namespace UserAuthService.Services.Interfaces
{
    public interface IHashingService
    {
        string Hash(string password);
    }
}
