using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Services
{
    public class RepositoryContainer : IRepositoryContainer
    {
        public ITransportOrderRepository OrderRepo { get; }
        public IRepository<ConsigneeDTO> ConsigneeRepo { get; }
        public IRepository<ShipperDTO> ShipperRepo { get; }
        public IRepository<CargoDTO> CargoRepo { get; }
        public IRouteRepository RouteRepo { get; }
        public IRepository<VehicleDTO> VehicleRepo { get; }

        public RepositoryContainer(ITransportOrderRepository orderRepo, IRepository<ConsigneeDTO> consigneeRepo, IRepository<ShipperDTO> shipperRepo, IRepository<CargoDTO> cargoRepo, IRouteRepository routeRepo,IRepository<VehicleDTO> vehicleRepo)
        {
            OrderRepo = orderRepo;
            ConsigneeRepo = consigneeRepo;
            ShipperRepo = shipperRepo;
            CargoRepo = cargoRepo;
            RouteRepo = routeRepo;
            VehicleRepo = vehicleRepo;
        }
    }
}
