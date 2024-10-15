using System.Text;
using TranzLog.Models;

namespace TranzLog.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ShippingDbContext context)
        {
            if(context.Users.Any())
            {
                return;
            }
            User admin = new User
            {
                Role = Role.Administrator,
                UserName = "admin",
                CreatedDate = DateTime.UtcNow,
                Password = Encoding.UTF8.GetBytes("admin")
            };
            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
