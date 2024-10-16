using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IAuthenticationService
    {
        string AuthenticateAsync(LoginDTO loginDTO);
        Task<RegistrationResult> RegisterAsync(RegisterDTO registerDto);
    }
}
