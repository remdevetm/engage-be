using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Text;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Services
{
    public class HashingService : IHashingService
    {
        public string Hash(string password)
        {
            string hashed = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                      password: password!,
                      salt: Encoding.ASCII.GetBytes("8344f86a166d7e5db768ca27b34250c05a0ae243f94a73a1582fadaf634123af"),
                      prf: KeyDerivationPrf.HMACSHA256,
                      iterationCount: 100000,
                      numBytesRequested: 256 / 8));
            return hashed;
        }
    }
}
