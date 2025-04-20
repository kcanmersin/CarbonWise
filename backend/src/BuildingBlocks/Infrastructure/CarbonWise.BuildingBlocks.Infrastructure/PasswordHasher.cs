using System;
using System.Security.Cryptography;
using CarbonWise.BuildingBlocks.Application.Security;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CarbonWise.BuildingBlocks.Infrastructure.Security
{
    public class PasswordHasher : IPasswordHasher
    {
        private const int SaltSize = 128 / 8;
        private const int KeySize = 256 / 8;
        private const int Iterations = 10000;
        private static readonly HashAlgorithmName _hashAlgorithmName = HashAlgorithmName.SHA256;
        private const char Delimiter = ';';

        public string HashPassword(string password)
        {
            var salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            var hash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                Iterations,
                _hashAlgorithmName,
                KeySize);

            return string.Join(Delimiter, Convert.ToBase64String(salt), Convert.ToBase64String(hash), Iterations, _hashAlgorithmName);
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            var parts = passwordHash.Split(Delimiter);
            if (parts.Length != 4)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);
            var iterations = int.Parse(parts[2]);
            var hashAlgorithmName = new HashAlgorithmName(parts[3]);

            var checkHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                hashAlgorithmName,
                hash.Length);

            return CryptographicOperations.FixedTimeEquals(hash, checkHash);
        }
    }
}