using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class DriverDTO
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string LicenseNumber { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public DateTime BirthDate { get; set; }
    }
}
