using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class TransportOrderDTO
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public int? ShipperId { get; set; }
        public int? ConsigneeId { get; set; }
        public int? RouteId { get; set; }
        public int? VehicleId { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? StartTransportTime { get; set; }
        public DateTime? PlannedDeliveryTime { get; set; }
        public string? Notes { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? TrackNumber { get; set; }
    }
}
