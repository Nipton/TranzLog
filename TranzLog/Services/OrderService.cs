using AutoMapper;
using Microsoft.EntityFrameworkCore;
using TranzLog.Data;
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
        private readonly IAuthenticationService authenticationService;
        public OrderService(ShippingDbContext db, IMapper mapper, ITransportOrderRepository orderRepo, IRepository<ConsigneeDTO> consigneeRepo, IRepository<ShipperDTO> shipperRepo, IRepository<CargoDTO> cargoRepo, IUserRepository userRepo, IRouteRepository routeRepo, IAuthenticationService authenticationService)
        {
            this.mapper = mapper;
            this.orderRepo = orderRepo;
            this.consigneeRepo = consigneeRepo;
            this.shipperRepo = shipperRepo;    
            this.cargoRepo = cargoRepo;
            this.userRepo = userRepo;
            this.routeRepo = routeRepo;
            this.authenticationService = authenticationService;
            this.db = db;
        }
        public async Task<string> CreateOrder(UserOrderDTO userOrderDTO, HttpContext httpContext)
        {
            if (userOrderDTO.Consignee == null || userOrderDTO.Shipper == null || userOrderDTO.RouteId == null || userOrderDTO.CargoList.Count < 1)
            {
                throw new ArgumentException("Неполные данные для создания заказа.");
            }
            if(!await routeRepo.RouteExistsAsync((int)userOrderDTO.RouteId))
                throw new ArgumentException("Несуществующий путь.");
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
        //private async Task<UserDTO> GetCurrentUser(HttpContext httpContext)
        //{
        //    User? currentUser = authenticationService.GetCurrentUserInfo(httpContext);
        //    if (currentUser == null)
        //        throw new UnauthorizedAccessException("Ошибка аутентификации.");
        //    var currentUserWithId = await userRepo.GetUserByNameAsync(currentUser.UserName);
        //    if (currentUserWithId == null)
        //        throw new UnauthorizedAccessException("Ошибка аутентификации.");
        //    return currentUserWithId;
        //}
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
