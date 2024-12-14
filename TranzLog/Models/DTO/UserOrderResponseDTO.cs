namespace TranzLog.Models.DTO
{
    public class UserOrderResponseDTO
    {
        public int Id { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? CompletionTime { get; set; }
        public DateTime? StartTransportTime { get; set; }
        public DateTime? PlannedDeliveryTime { get; set; }
        public double? DeliveryCost { get; set; }
        public string? Notes { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public string? TrackNumber { get; set; }
        public ShipperDTO? Shipper { get; set; }
        public ConsigneeDTO? Consignee { get; set; }
        public RouteDTO? Route { get; set; }
        public List<CargoDTO>? Cargo { get; set; } 
    }
}
