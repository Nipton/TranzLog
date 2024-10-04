using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class CargoDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string? Type { get; set; }
        public double Volume { get; set; }
        public double Weight { get; set; }
        public string? PackagingType { get; set; }
    }
}
