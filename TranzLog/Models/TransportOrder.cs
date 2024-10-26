using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    [Index(nameof(TrackNumber), IsUnique = true)]
    public class TransportOrder
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public virtual Shipper? Shipper { get; set; }
        public int? ShipperId { get; set; }
        public virtual Consignee? Consignee { get; set; }
        public int? ConsigneeId { get; set; }

        public virtual Route? Route { get; set; }
        public int? RouteId { get; set; }

        public virtual Vehicle? Vehicle { get; set; }
        public int? VehicleId { get; set; }
        public virtual List<Cargo>? Cargo { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? StartTransportTime { get; set; } 
        public DateTime? PlannedDeliveryTime { get; set; }
        public string? Notes { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? TrackNumber { get; set; }
    }
}
