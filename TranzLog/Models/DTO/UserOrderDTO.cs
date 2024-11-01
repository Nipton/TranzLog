﻿using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models.DTO
{
    public class UserOrderDTO
    {
        public ShipperDTO? Shipper { get; set; }
        public ConsigneeDTO? Consignee { get; set; }
        public int? RouteId { get; set; }
        public List<CargoDTO> CargoList { get; set; } = new List<CargoDTO>();
        public string? Notes { get; set; }
    }
}
