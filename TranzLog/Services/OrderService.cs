using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TranzLog.Data;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Services
{
    public class OrderService : IOrderService
    {

        private readonly IMapper mapper;
        private readonly ShippingDbContext db;
        private readonly ITransportOrderRepository orderRepo;
        private readonly IRepository<ConsigneeDTO> consigneeRepo;
        private readonly IRepository<ShipperDTO> shipperRepo;
        private readonly IRepository<CargoDTO> cargoRepo;
        private readonly IUserRepository userRepo;
        private readonly IRouteRepository routeRepo;
        private readonly IRepository<VehicleDTO> vehicleRepo;
        private readonly IAuthenticationService authenticationService;
        public OrderService(ShippingDbContext db, IMapper mapper, ITransportOrderRepository orderRepo, IRepository<ConsigneeDTO> consigneeRepo, IRepository<ShipperDTO> shipperRepo, IRepository<CargoDTO> cargoRepo, IUserRepository userRepo, IRouteRepository routeRepo, IAuthenticationService authenticationService, IRepository<VehicleDTO> vehicleRepo)
        {
            this.mapper = mapper;
            this.orderRepo = orderRepo;
            this.consigneeRepo = consigneeRepo;
            this.shipperRepo = shipperRepo;    
            this.cargoRepo = cargoRepo;
            this.userRepo = userRepo;
            this.routeRepo = routeRepo;
            this.authenticationService = authenticationService;
            this.vehicleRepo = vehicleRepo;
            this.db = db;
        }
        
        public async Task<TransportOrderDTO> CreateOrderAsync(TransportOrderDTO orderDTO)
        {
            await ValidateRelatedEntitiesAsync(orderDTO);
            orderDTO.TrackNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12).ToUpper();
            var order = await orderRepo.AddAsync(orderDTO);
            return order;
        }
        public async Task<TransportOrderDTO> UpdateOrderAsync(TransportOrderDTO orderDTO)
        {
            await ValidateRelatedEntitiesAsync(orderDTO);
            var order = await orderRepo.UpdateAsync(orderDTO);
            return order;
        }
        public async Task<ActionResult<TransportOrderDTO?>> GetOrderAsync(int id)
        {
            var order = await orderRepo.GetAsync(id);
            return order;
        }
        public IEnumerable<TransportOrderDTO> GetAll(int page, int pageSize)
        {
            var orders = orderRepo.GetAll(page, pageSize);
            return orders;
        }
        public async Task DeleteAsync(int id)
        {
            await orderRepo.DeleteAsync(id);
        }
        private async Task ValidateRelatedEntitiesAsync(TransportOrderDTO orderDTO)
        {
            if (orderDTO.ConsigneeId != null)
            {
                var consignee = await consigneeRepo.GetAsync((int)orderDTO.ConsigneeId);
                if (consignee == null)
                    throw new EntityNotFoundException($"Получатель с ID {orderDTO.ConsigneeId} не найден.");
            }
            if (orderDTO.ShipperId != null)
            {
                var shipper = await shipperRepo.GetAsync((int)orderDTO.ShipperId);
                if (shipper == null)
                    throw new EntityNotFoundException($"Отправитель с ID {orderDTO.ShipperId} не найден.");
            }
            if (orderDTO.RouteId != null && !await routeRepo.RouteExistsAsync((int)orderDTO.RouteId))
            {
                throw new EntityNotFoundException($"Маршрут с ID {orderDTO.RouteId} не найден.");
            }
            if  (orderDTO.VehicleId != null)
            {
                var vehicle = await vehicleRepo.GetAsync((int) orderDTO.VehicleId);
                if (vehicle == null)
                    throw new EntityNotFoundException($"Транспорт с ID {orderDTO.VehicleId} не найден.");
            }
        }
        public async Task<string> CreateOrderByUserAsync(UserOrderRequestDTO userOrderDTO, HttpContext httpContext)
        {
            if (userOrderDTO.Consignee == null || userOrderDTO.Shipper == null || userOrderDTO.RouteId == null || userOrderDTO.CargoList.Count < 1)
            {
                throw new InvalidParameterException("Неполные данные для создания заказа.");
            }
            if(!await routeRepo.RouteExistsAsync((int)userOrderDTO.RouteId))
                throw new EntityNotFoundException("Несуществующий путь.");
            TransportOrderDTO order = new TransportOrderDTO();
            var currentUserWithId = await authenticationService.GetCurrentUserAsync(httpContext);
            order.UserId = currentUserWithId.Id;
            order.OrderStatus = OrderStatus.Pending;
            order.RouteId = userOrderDTO.RouteId;
            order.CreatedAt = DateTime.UtcNow;
            order.Notes = userOrderDTO.Notes;
            order.TrackNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12).ToUpper();
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    var consignee = await consigneeRepo.AddAsync(userOrderDTO.Consignee);
                    var shipper = await shipperRepo.AddAsync(userOrderDTO.Shipper);
                    order.ShipperId = shipper.Id;
                    order.ConsigneeId = consignee.Id;
                    var orderResult = await orderRepo.AddAsync(order);
                    foreach (var cargo in userOrderDTO.CargoList)
                    {
                        cargo.TransportOrderId = orderResult.Id;
                        await cargoRepo.AddAsync(cargo);
                    }
                    transaction.Commit();
                    return orderResult.TrackNumber!;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }           
        }
        public async Task<UserOrderResponseDTO?> GetOrderInfoByTrackerAsync(string trackNumber)
        {
            TransportOrder? transportOrder = await orderRepo.GetOrderInfoByTrackerAsync(trackNumber);
            if (transportOrder != null)
            {
                var orderDTO = mapper.Map<UserOrderResponseDTO>(transportOrder);
                return orderDTO;
            }
            return null;
        }
        public async Task<List<UserOrderResponseDTO>> GetUserOrdersAsync(HttpContext httpContext)
        {
            var user = await authenticationService.GetCurrentUserAsync(httpContext);
            var orders = await orderRepo.GetUserOrdersByIdAsync(user.Id);
            var ordersDTO = orders.Select(order => mapper.Map<UserOrderResponseDTO>(order)).ToList();
            return ordersDTO;
        }
        public async Task CancelOrderAsync(int orderId, HttpContext httpContext)
        {
            var order = await orderRepo.GetAsync(orderId);
            if (order == null)
                throw new EntityNotFoundException($"Заказ с ID {orderId} не найден.");
            var user = await authenticationService.GetCurrentUserAsync(httpContext);
            if (user.Id != order.UserId)
            {
                throw new UnauthorizedAccessException("Нет прав для данного действия.");
            }
            order.OrderStatus = OrderStatus.Cancelled;
            await orderRepo.UpdateAsync(order);
        }
    }
}
