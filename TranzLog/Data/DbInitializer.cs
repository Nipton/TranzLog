using System.Text;
using TranzLog.Interfaces;
using TranzLog.Models;

namespace TranzLog.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ShippingDbContext context, IPasswordHasher passwordHasher)
        {
            if(context.Users.Any())
            {
                return;
            }
            var salt = new byte[16];
            new Random().NextBytes(salt);
            User admin = new User
            {
                Role = Role.Administrator,
                UserName = "admin",
                CreatedDate = DateTime.UtcNow,
                Salt = salt,
                Password = passwordHasher.HashPassword("admin", salt)
            };
            context.Users.Add(admin);
            context.SaveChanges();
        }
    }
}
