using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = "";
        public Role Role { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required]
        public byte[] Password { get; set; } = null!;
        public byte[] Salt { get; set; } = null!;
       
    }
}
