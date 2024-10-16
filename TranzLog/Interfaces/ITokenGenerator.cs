using TranzLog.Models;

namespace TranzLog.Interfaces
{
    public interface ITokenGenerator
    {
        string GetToken(User user);
    }
}
