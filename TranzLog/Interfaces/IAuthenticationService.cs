using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string> AuthenticateAsync(LoginDTO loginDTO);
        Task<RegistrationResult> RegisterAsync(RegisterDTO registerDto);
        Task ChangeUserRole(string userName, string targetRole, string roleCurrentUser);
        User? GetCurrentUserInfo(HttpContext httpContext);
        Task<UserDTO> GetCurrentUserAsync(HttpContext httpContext);
    }
}
