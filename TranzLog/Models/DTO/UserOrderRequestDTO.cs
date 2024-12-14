using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class UserOrderRequestDTO
    {
        public ShipperDTO? Shipper { get; set; }
        public ConsigneeDTO? Consignee { get; set; }
        public RouteDTO? Route { get; set; }
        public List<CargoDTO> CargoList { get; set; } = new List<CargoDTO>();
        public string? Notes { get; set; }
    }
}
