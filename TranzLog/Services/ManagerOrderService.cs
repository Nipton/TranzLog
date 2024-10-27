using AutoMapper;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Services
{
    public class ManagerOrderService : IManagerOrderService
    {
        private readonly IMapper mapper;
        private readonly ITransportOrderRepository orderRepo;
        private readonly ILogger<ManagerOrderService> logger;
        public ManagerOrderService(IMapper mapper, ITransportOrderRepository orderRepo, ILogger<ManagerOrderService> logger)
        {
            this.mapper = mapper;
            this.orderRepo = orderRepo;
            this.logger = logger;
        }

        public async Task<List<UserOrderResponseDTO>> GetPendingOrdersAsync()
        {
            var ordersPending = await orderRepo.GetPendingOrdersAsync();
            var ordersDTO = ordersPending.Select(order => mapper.Map<UserOrderResponseDTO>(order)).ToList();
            return ordersDTO;
        }
        public async Task UpdateOrderStatusAsync(int orderId, int newStatus)
        {
            if (!Enum.IsDefined(typeof(OrderStatus), newStatus))
            {
                throw new InvalidParameterException("Недопустимый статус заказа.");
            }
            var orderStatus = (OrderStatus)newStatus;
            await orderRepo.UpdateOrderStatusAsync(orderId, orderStatus);
        }
    }
}
