using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    public class Cargo
    {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; } = "";
        public string? Type { get; set; }
        public double Volume { get; set; }
        public double Weight { get; set; }
        public string? PackagingType { get; set; }
        public virtual ICollection<TransportOrder>? TransportOrders { get; set; }
    }
}
