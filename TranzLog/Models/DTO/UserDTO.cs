using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class UserDTO
    {
        public int Id { get; set; }
        public string UserName { get; set; } = "";
        public Role Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Password { get; set; } = null!;
    }
}