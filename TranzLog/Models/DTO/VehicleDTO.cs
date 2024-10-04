using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class VehicleDTO
    {
        public int Id { get; set; }
        public string LicensePlateNumber { get; set; } = "";
        public string Make { get; set; } = "";
        public string Model { get; set; } = "";
        public int Capacity { get; set; }
        public int? DriverId { get; set; }
    }
}
