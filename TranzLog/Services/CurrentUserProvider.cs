using System.Security.Claims;
using TranzLog.Models;

namespace TranzLog.Services
{
    public class CurrentUserProvider
    {
        /// <summary>
        /// Возвращает текущего пользователя с его именем и ролью.
        /// </summary>
        /// <returns>Объект, содержащий имя и роль текущего пользователя.</returns>
        public static User? GetCurrentUserInfo(HttpContext httpContext)
        {
            var identity = httpContext.User.Identity as ClaimsIdentity;
            if (identity != null)
            {
                var userName = identity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var userRole = identity.FindFirst(ClaimTypes.Role)?.Value;
                if (userName != null && userRole != null)
                {
                    User user = new User { UserName = userName, Role = (Role)Enum.Parse(typeof(Role), userRole) };
                    return user;
                }
            }
            return null;
        }

    }
}
