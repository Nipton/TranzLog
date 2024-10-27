using AutoMapper;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Services
{
    public class DriverOrderService : IDriverOrderService
    {
        private readonly IMapper mapper;
        private readonly ITransportOrderRepository orderRepo;
        private readonly ILogger<DriverOrderService> logger;
        private readonly IAuthenticationService authenticationService;
        private readonly IDriverRepository driverRepository;
        private readonly IRepository<VehicleDTO> vehicleRepository;
        public DriverOrderService(IMapper mapper, ITransportOrderRepository orderRepo, ILogger<DriverOrderService> logger, IAuthenticationService authenticationService, IDriverRepository driverRepository, IRepository<VehicleDTO> vehicleRepository)
        {
            this.mapper = mapper;
            this.orderRepo = orderRepo;
            this.logger = logger;
            this.authenticationService = authenticationService;
            this.driverRepository = driverRepository;
            this.vehicleRepository = vehicleRepository;
        }

        public async Task<List<DriverOrderDTO>> GetDriverAssignedOrdersAsync(HttpContext httpContext)
        {
            var currentUser = await authenticationService.GetCurrentUserAsync(httpContext);
            var driver = await driverRepository.FindDriverByUserIdAsync(currentUser.Id);
            if (driver == null)
                throw new UserNotFoundException($"Запись о водителе с ID {currentUser.Id} не найдена.");
            var orders = await orderRepo.GetOrdersForDriverAsync(driver.Id);
            return orders;
        }

        public async Task UpdateOrderDeliveryStatusAsync(int orderId, int newStatus, HttpContext httpContext)
        {
            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
            {
                throw new InvalidParameterException("Недопустимый статус заказа.");
            }
            var orderStatus = (OrderStatus)newStatus;
            if (!(orderStatus == OrderStatus.AcceptedByDriver || orderStatus == OrderStatus.Cancelled))
                throw new InvalidParameterException("Статус заказа можно изменить только на 'AcceptedByDriver' или 'Cancelled'");

            var currentUser = await authenticationService.GetCurrentUserAsync(httpContext);
            var driver = await driverRepository.FindDriverByUserIdAsync(currentUser.Id);
            if (driver == null)
                throw new UserNotFoundException($"Запись о водителе с ID {currentUser.Id} не найдена.");
            var order = await orderRepo.GetAsync(orderId);
            if (order == null)
                throw new EntityNotFoundException($"Заказ с ID {orderId} не найден.");
            if (order.VehicleId == null)
                throw new ArgumentException("В заказе не указан транспорт и водитель.");
            var vehicle = await vehicleRepository.GetAsync((int)order.VehicleId);
            if (vehicle == null)
                throw new EntityNotFoundException($"Транспорт с ID {order.VehicleId} не найден.");
            if (vehicle.DriverId != driver.Id)
                throw new AccessDeniedException("Недостаточно прав для данного действия.");
            await orderRepo.UpdateOrderStatusAsync(orderId, orderStatus);
        }
    }
}
