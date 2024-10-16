namespace TranzLog.Interfaces
{
    public interface IPasswordHasher
    {
        byte[] HashPassword(string password, byte[] salt);
        bool VerifyPassword(string password, byte[] hashedPassword, byte[] salt);
    }
}
