using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    public class Vehicle
    {
        public int Id { get; set; }
        [Required]
        [MaxLength(20)]
        public string LicensePlateNumber { get; set; } = "";
        [Required]
        public string Make { get; set; } = "";

        [Required]
        public string Model { get; set; } = "";

        [Required]
        public int Capacity { get; set; }

        public Driver? Driver { get; set; }
        public int? DriverId { get; set; }
    }
}
