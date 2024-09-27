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
        public TimeSpan EstimatedDuration { get; set; }
        public bool IsActive { get; set; }
    }
}
