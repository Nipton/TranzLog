namespace TranzLog.Models.DTO
{
    public class DriverOrderDTO
    {
        public int Id { get; set; }
        public RouteDTO? Route { get; set; }
        public List<CargoDTO>? Cargo { get; set; }
        public DateTime? StartTransportTime { get; set; }
        public DateTime? PlannedDeliveryTime { get; set; }
        public string? Notes { get; set; }
        public OrderStatus OrderStatus { get; set; }
        public VehicleDTO? Vehicle { get; set; }
    }
}
