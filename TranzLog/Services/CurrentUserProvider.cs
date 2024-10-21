using System.Security.Claims;
using TranzLog.Models;

namespace TranzLog.Services
{
    public class CurrentUserProvider
    {
        public static User? GetCurrentUser(HttpContext httpContext)
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
