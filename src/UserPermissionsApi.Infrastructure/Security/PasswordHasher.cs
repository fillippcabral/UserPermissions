
using System.Security.Cryptography;
using UserPermissions.Application.Interfaces;

namespace UserPermissions.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public void CreateHash(string password, out string hash, out string salt)
    {
        var saltBytes = RandomNumberGenerator.GetBytes(16);
        var hashBytes = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
        
        hash = Convert.ToBase64String(hashBytes);
        salt = Convert.ToBase64String(saltBytes);
    }

    public bool Verify(string password, string hash, string salt)
    {
        var saltBytes = Convert.FromBase64String(salt);
        var computed = Rfc2898DeriveBytes.Pbkdf2(password, saltBytes, 100_000, HashAlgorithmName.SHA256, 32);
        
        return CryptographicOperations.FixedTimeEquals(computed, Convert.FromBase64String(hash));
    }
}
