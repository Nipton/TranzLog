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
        private readonly IRepositoryContainer repoContainer;
        private readonly ICostCalculationService costCalculation;
        private readonly IDistanceCalculationService distanceCalculation;
        public ManagerOrderService(IMapper mapper, IRepositoryContainer repoContainer, ICostCalculationService costCalculation, IDistanceCalculationService distanceCalculation)
        {
            this.mapper = mapper;
            this.repoContainer = repoContainer;
            this.costCalculation = costCalculation;
            this.distanceCalculation = distanceCalculation;
        }

        public async Task<List<UserOrderResponseDTO>> GetPendingOrdersAsync()
        {
            var ordersPending = await repoContainer.OrderRepo.GetPendingOrdersAsync();
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
            await repoContainer.OrderRepo.UpdateOrderStatusAsync(orderId, orderStatus);
        }
        public async Task ConfirmOrderAsync(ConfirmOrderRequestDTO request)
        {
            if (request.StartTransportTime == default)
            {
                throw new InvalidOperationException("Начало транспортировки не задано.");
            }
            var order = await repoContainer.OrderRepo.GetAsync(request.OrderId) ?? throw new EntityNotFoundException("Заказ не найден");
            if(order.OrderStatus != OrderStatus.Pending)
                throw new InvalidOperationException("Только заказы со статусом 'Pending' можно подтвердить");          
            if(order.RouteId == null)
                throw new InvalidOperationException("Невозможно подтвердить заказ без указания маршрута.");
            if(order.DeliveryCost == null)
                throw new InvalidOperationException("Невозможно подтвердить заказ без указания стоимости доставки.");
            var route = await repoContainer.RouteRepo.GetAsync((int)order.RouteId) ?? throw new EntityNotFoundException("Маршрут не найден. Невозможно подтвердить заказ без указания маршрута.");
            order.StartTransportTime = request.StartTransportTime;
            order.PlannedDeliveryTime = request.StartTransportTime.Add(route.EstimatedDuration);
            order.OrderStatus = OrderStatus.Confirmed;
            await repoContainer.OrderRepo.UpdateAsync(order);
        }
        public async Task UpdateDeliveryCost(int orderId)
        {
            var order = await repoContainer.OrderRepo.GetAsync(orderId) ?? throw new EntityNotFoundException("Заказ не найден");
            if(order.Cargo == null)
                throw new EntityNotFoundException("Грузы для заказа не найдены.");
            if (order.RouteId == null)
                throw new EntityNotFoundException("Маршрут для заказа не найден.");
            var route = await repoContainer.RouteRepo.GetAsync((int)order.RouteId) ?? throw new EntityNotFoundException("Маршрут для заказа не найден.");
            var distance = await distanceCalculation.CalculateDistanceAsync(mapper.Map<Models.Route>(route));
            order.DeliveryCost = costCalculation.CalculateCost(distance.Distance, order.Cargo.Select(x => mapper.Map<Cargo>(x)).ToList());
            if(order.StartTransportTime != default && order.StartTransportTime != null && route.EstimatedDuration != default)
            {
                DateTime date = (DateTime)order.StartTransportTime;
                order.PlannedDeliveryTime = date.Add(route.EstimatedDuration);
            }
            await repoContainer.OrderRepo.UpdateAsync(order);
        }
    }
}
