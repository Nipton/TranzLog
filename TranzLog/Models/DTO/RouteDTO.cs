using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class RouteDTO
    {
        public int Id { get; set; }
        public string Origin { get; set; } = "";
        public string Destination { get; set; } = "";
        public TimeSpan EstimatedDuration { get; set; }
        public bool IsActive { get; set; }
    }
}
