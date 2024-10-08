﻿using System.ComponentModel.DataAnnotations;

namespace TranzLog.Models
{
    public class TransportOrder
    {
        public int Id { get; set; }
        public virtual Shipper? Shipper { get; set; }
        public int? ShipperId { get; set; }
        [Required]
        public virtual Consignee? Consignee { get; set; }
        public int? ConsigneeId { get; set; }

        public virtual Route? Route { get; set; }
        public int? RouteId { get; set; }

        public virtual Vehicle? Vehicle { get; set; }
        public int? VehicleId { get; set; }
        public virtual Cargo? Cargo { get; set; }
        public int? CargoId { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? CompletionTime { get; set; }
        public string? Notes { get; set; }
        public OrderStatus OrderStatus { get; set; }

        public TransportOrder() { }
        public TransportOrder(Shipper shipper, Consignee consignee, Route route, Vehicle vehicle, Cargo? cargo)
        {
            Shipper = shipper;
            Consignee = consignee;
            Route = route;
            Vehicle = vehicle;
            Cargo = cargo;
        }
    }
}
