using System.Security.Cryptography;
using System.Text;
using TranzLog.Interfaces;

namespace TranzLog.Services.AuthenticationServices
{
    public class PasswordHasher : IPasswordHasher
    {
        public byte[] HashPassword(string password, byte[] salt)
        {
            var data = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
            using (var sha512 = SHA512.Create())
            {
                return sha512.ComputeHash(data);
            }
        }

        public bool VerifyPassword(string password, byte[] hashedPassword, byte[] salt)
        {
            var data = Encoding.UTF8.GetBytes(password).Concat(salt).ToArray();
            using (var sha512 = SHA512.Create())
            {
                var hashToCompare = sha512.ComputeHash(data);
                return hashToCompare.SequenceEqual(hashedPassword);
            }
        }
    }
}
