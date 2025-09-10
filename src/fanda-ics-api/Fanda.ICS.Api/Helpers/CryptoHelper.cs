using System.Security.Cryptography;

using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace Fanda.ICS.Api.Helpers;

public class CryptoHelper
{
    public static byte[] GenerateSalt(int saltSize = 16)
    {
        byte[] salt = new byte[saltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    public static (string, string) HashPassword(string password, int iterations = 10000,
        KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256, int hashSize = 32)
    {
        byte[] salt = GenerateSalt();
        return HashPassword(password, salt, iterations, prf, hashSize);
    }

    public static (string, string) HashPassword(string password, byte[] salt, int iterations = 10000,
        KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256, int hashSize = 32)
    {
        byte[] hash = KeyDerivation.Pbkdf2(
            password: password,
            salt: salt,
            prf: prf,
            iterationCount: iterations,
            numBytesRequested: hashSize);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public static bool VerifyPassword(string password, string storedHash, string storedSalt,
        int iterations = 10000, KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA256, int hashSize = 32)
    {
        byte[] salt = Convert.FromBase64String(storedSalt);
        string newHash = HashPassword(password, salt, iterations, prf, hashSize).Item1;
        return newHash.Equals(storedHash); // Use a constant-time comparison in a real application
    }
}
