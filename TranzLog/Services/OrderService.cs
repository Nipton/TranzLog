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
        private readonly ITransactionManager transactionManager;
        private readonly IAuthenticationService authenticationService;
        private readonly IRepositoryContainer repoContainer;
        public OrderService(ITransactionManager transactionManager, IMapper mapper, IRepositoryContainer repoContainer,  IAuthenticationService authenticationService)
        {
            this.mapper = mapper;
            this.authenticationService = authenticationService;
            this.transactionManager = transactionManager;
            this.repoContainer = repoContainer;
        }
        
        public async Task<TransportOrderDTO> CreateOrderAsync(TransportOrderDTO orderDTO)
        {
            await ValidateRelatedEntitiesAsync(orderDTO);
            orderDTO.TrackNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12).ToUpper();
            var order = await repoContainer.OrderRepo.AddAsync(orderDTO);
            return order;
        }
        public async Task<TransportOrderDTO> UpdateOrderAsync(TransportOrderDTO orderDTO)
        {
            await ValidateRelatedEntitiesAsync(orderDTO);
            var order = await repoContainer.OrderRepo.UpdateAsync(orderDTO);
            return order;
        }
        public async Task<ActionResult<TransportOrderDTO?>> GetOrderAsync(int id)
        {
            var order = await repoContainer.OrderRepo.GetAsync(id);
            return order;
        }
        public IEnumerable<TransportOrderDTO> GetAll(int page, int pageSize)
        {
            var orders = repoContainer.OrderRepo.GetAll(page, pageSize);
            return orders;
        }
        public async Task DeleteAsync(int id)
        {
            await repoContainer.OrderRepo.DeleteAsync(id);
        }
        private async Task ValidateRelatedEntitiesAsync(TransportOrderDTO orderDTO)
        {
            if (orderDTO.ConsigneeId != null)
            {
                var consignee = await repoContainer.ConsigneeRepo.GetAsync((int)orderDTO.ConsigneeId);
                if (consignee == null)
                    throw new EntityNotFoundException($"Получатель с ID {orderDTO.ConsigneeId} не найден.");
            }
            if (orderDTO.ShipperId != null)
            {
                var shipper = await repoContainer.ShipperRepo.GetAsync((int)orderDTO.ShipperId);
                if (shipper == null)
                    throw new EntityNotFoundException($"Отправитель с ID {orderDTO.ShipperId} не найден.");
            }
            if (orderDTO.RouteId != null && !await repoContainer.RouteRepo.RouteExistsAsync((int)orderDTO.RouteId))
            {
                throw new EntityNotFoundException($"Маршрут с ID {orderDTO.RouteId} не найден.");
            }
            if  (orderDTO.VehicleId != null)
            {
                var vehicle = await repoContainer.VehicleRepo.GetAsync((int) orderDTO.VehicleId);
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
            if(!await repoContainer.RouteRepo.RouteExistsAsync((int)userOrderDTO.RouteId))
                throw new EntityNotFoundException("Несуществующий путь.");
            TransportOrderDTO order = new TransportOrderDTO();
            var currentUserWithId = await authenticationService.GetCurrentUserAsync(httpContext);
            order.UserId = currentUserWithId.Id;
            order.OrderStatus = OrderStatus.Pending;
            order.RouteId = userOrderDTO.RouteId;
            order.CreatedAt = DateTime.UtcNow;
            order.Notes = userOrderDTO.Notes;
            order.TrackNumber = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 12).ToUpper();
            using (var transaction = transactionManager.BeginTransaction())
            {
                try
                {
                    var consignee = await repoContainer.ConsigneeRepo.AddAsync(userOrderDTO.Consignee);
                    var shipper = await repoContainer.ShipperRepo.AddAsync(userOrderDTO.Shipper);
                    order.ShipperId = shipper.Id;
                    order.ConsigneeId = consignee.Id;
                    var orderResult = await repoContainer.OrderRepo.AddAsync(order);
                    foreach (var cargo in userOrderDTO.CargoList)
                    {
                        cargo.TransportOrderId = orderResult.Id;
                        await repoContainer.CargoRepo.AddAsync(cargo);
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
            TransportOrder? transportOrder = await repoContainer.OrderRepo.GetOrderInfoByTrackerAsync(trackNumber);
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
            var orders = await repoContainer.OrderRepo.GetUserOrdersByIdAsync(user.Id);
            var ordersDTO = orders.Select(order => mapper.Map<UserOrderResponseDTO>(order)).ToList();
            return ordersDTO;
        }
        public async Task CancelOrderAsync(int orderId, HttpContext httpContext)
        {
            var order = await repoContainer.OrderRepo.GetAsync(orderId);
            if (order == null)
                throw new EntityNotFoundException($"Заказ с ID {orderId} не найден.");
            var user = await authenticationService.GetCurrentUserAsync(httpContext);
            if (user.Id != order.UserId)
            {
                throw new UnauthorizedAccessException("Нет прав для данного действия.");
            }
            order.OrderStatus = OrderStatus.Cancelled;
            await repoContainer.OrderRepo.UpdateAsync(order);
        }
    }
}
