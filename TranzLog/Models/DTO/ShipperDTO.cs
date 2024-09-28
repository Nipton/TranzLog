using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class ShipperDTO
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = "";
        public string ContactPerson { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Email { get; set; } = "";
        public string Address { get; set; } = "";
    }
}
