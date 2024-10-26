using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class DriverDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; } = "";
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; } = "";
        public string LicenseNumber { get; set; } = "";
        [Phone]
        public string PhoneNumber { get; set; } = "";
        public DateTime BirthDate { get; set; }
    }
}
