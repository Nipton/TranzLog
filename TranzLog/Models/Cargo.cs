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
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? PackagingType { get; set; }
        public int? TransportOrderId { get; set; }
        public virtual TransportOrder? TransportOrder { get; set; }
    }
}
