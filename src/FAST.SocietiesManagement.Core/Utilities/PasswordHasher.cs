using System;
using System.Security.Cryptography;

namespace FAST.SocietiesManagement.Core.Utilities
{
    public static class PasswordHasher
    {
        private const int SaltSize = 16; // 128 bit
        private const int KeySize = 32; // 256 bit
        private const int Iterations = 100000;

        public static (string Hash, string Salt) HashPassword(string password)
        {
            var saltBytes = RandomNumberGenerator.GetBytes(SaltSize);
            var keyBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);

            return (Convert.ToBase64String(keyBytes), Convert.ToBase64String(saltBytes));
        }

        public static bool VerifyPassword(string password, string hash, string salt)
        {
            var saltBytes = Convert.FromBase64String(salt);

            var keyToCheck = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, Iterations, HashAlgorithmName.SHA256, KeySize);
            var expected = Convert.FromBase64String(hash);
            return CryptographicOperations.FixedTimeEquals(keyToCheck, expected);
        }
    }
}
