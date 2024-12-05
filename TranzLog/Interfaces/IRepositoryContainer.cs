using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IRepositoryContainer
    {
        ITransportOrderRepository OrderRepo { get; }
        IRepository<ConsigneeDTO> ConsigneeRepo { get; }
        IRepository<ShipperDTO> ShipperRepo { get; }
        IRepository<CargoDTO> CargoRepo { get; }
        IRouteRepository RouteRepo { get; }
        IRepository<VehicleDTO> VehicleRepo { get; }
    }
}
