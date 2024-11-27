using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using System.Text;
using UserAuthService.Services.Interfaces;

namespace UserAuthService.Services
{
    public class HashingService : IHashingService
    {
        private string GenerateRandomSalt()
        {
            var saltBytes = new byte[16]; // 128-bit salt
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(saltBytes);
            return Convert.ToBase64String(saltBytes);
        }

        public string Hash(string password, out string salt)
        {
            salt = GenerateRandomSalt();
            var hash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password,
                salt: Encoding.UTF8.GetBytes(salt),
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100_000,
                numBytesRequested: 32
            ));

            // Embed salt at a specific position within the hash
            int insertPosition = hash.Length / 2;
            return hash.Insert(insertPosition, $"${salt}$");
        }

        public bool Verify(string password, string storedHashWithSalt)
        {
            try
            {
                // Find the salt markers
                int firstMarker = storedHashWithSalt.IndexOf('$');
                int secondMarker = storedHashWithSalt.IndexOf('$', firstMarker + 1);

                // Validate marker positions
                if (firstMarker == -1 || secondMarker == -1 || firstMarker >= secondMarker)
                {
                    return false;
                }

                // Extract salt
                var salt = storedHashWithSalt.Substring(
                    firstMarker + 1,
                    secondMarker - firstMarker - 1
                );

                // Remove salt from the hash
                var cleanHash = storedHashWithSalt.Remove(firstMarker, secondMarker - firstMarker + 1);

                // Recompute hash
                var computedHash = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                    password: password,
                    salt: Encoding.UTF8.GetBytes(salt),
                    prf: KeyDerivationPrf.HMACSHA256,
                    iterationCount: 100_000,
                    numBytesRequested: 32
                ));

                // Compare hashes
                return computedHash == cleanHash;
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Verification error: {ex.Message}");
                return false;
            }
        }

    }

}
