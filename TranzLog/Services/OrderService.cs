using AutoMapper;
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
        private readonly IRepository<TransportOrderDTO> orderRepo;
        private readonly IRepository<ConsigneeDTO> consigneeRepo;
        private readonly IRepository<ShipperDTO> shipperRepo;
        private readonly IRepository<CargoDTO> cargoRepo;
        private readonly IUserRepository userRepo;
        public OrderService(ShippingDbContext db, IMapper mapper, IRepository<TransportOrderDTO> orderRepo, IRepository<ConsigneeDTO> consigneeRepo, IRepository<ShipperDTO> shipperRepo, IRepository<CargoDTO> cargoRepo, IUserRepository userRepo)
        {
            this.mapper = mapper;
            this.orderRepo = orderRepo;
            this.consigneeRepo = consigneeRepo;
            this.shipperRepo = shipperRepo;    
            this.cargoRepo = cargoRepo;
            this.userRepo = userRepo;
            this.db = db;
        }
        public async Task<int> CreateOrder(UserOrderDTO userOrderDTO, HttpContext httpContext)
        {
            if (userOrderDTO.Consignee == null || userOrderDTO.Shipper == null || userOrderDTO.RouteId == null || userOrderDTO.CargoList.Count < 1)
            {
                throw new ArgumentException("Неполные данные для создания заказа.");
            }
            TransportOrderDTO order = new TransportOrderDTO();
            User? currentUser = CurrentUserProvider.GetCurrentUserInfo(httpContext); 
            if (currentUser == null)
                throw new UnauthorizedAccessException("Ошибка аутентификации.");
            var currentUserWithId = await userRepo.GetUserByNameAsync(currentUser.UserName);
            if (currentUserWithId == null)
                throw new UnauthorizedAccessException("Ошибка аутентификации.");
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
                    return orderResult.Id;
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }           
        }
    }
}
