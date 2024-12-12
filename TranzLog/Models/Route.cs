using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    public class Route
    {
        public int Id { get; set; }
        [Required]
        public string Origin { get; set; } = "";
        [Required]
        public string Destination { get; set; } = "";
        public double OriginLatitude { get; set; } 
        public double OriginLongitude { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
        public double Distance { get; set; }
        public TimeSpan EstimatedDuration { get; set; }
        public bool IsActive { get; set; }
    }
}
