using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class TransportOrderRepository : ITransportOrderRepository
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private IMemoryCache cache;
        private const string CacheKeyPrefix = "order_";
        private static int CacheVersion = 0;
        public TransportOrderRepository(ShippingDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<TransportOrderDTO> AddAsync(TransportOrderDTO entityDTO)
        {
            TransportOrder order = mapper.Map<TransportOrder>(entityDTO);
            await db.TransportOrders.AddAsync(order);
            await db.SaveChangesAsync();
            Interlocked.Increment(ref CacheVersion);
            return mapper.Map<TransportOrderDTO>(order);
        }

        public async Task<TransportOrderDTO> UpdateAsync(TransportOrderDTO entityDTO)
        {
            TransportOrder? order = await db.TransportOrders.FindAsync(entityDTO.Id);
            if (order != null)
            {
                mapper.Map(entityDTO, order);
                await db.SaveChangesAsync();
                string cacheKey = CacheKeyPrefix + entityDTO.Id;
                cache.Set(cacheKey, entityDTO, TimeSpan.FromMinutes(360));
                Interlocked.Increment(ref CacheVersion);
                return mapper.Map<TransportOrderDTO>(order);
            }
            else
            {
                throw new EntityNotFoundException($"Заказ с ID {entityDTO} не найден.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            TransportOrder? order = await db.TransportOrders.FindAsync(id);
            if (order != null)
            {
                db.TransportOrders.Remove(order);
                await db.SaveChangesAsync();
                cache.Remove(CacheKeyPrefix + id);
                Interlocked.Increment(ref CacheVersion);
            }
            else
            {
                throw new EntityNotFoundException($"Заказ с ID {id} не найден.");
            }
        }

        public async Task<TransportOrderDTO?> GetAsync(int id)
        {
            if (id <= 0)
                throw new InvalidParameterException($"Передано некорректное значение ID: {id}");
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out TransportOrderDTO? cacheResult))
            {
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            TransportOrder? order = await db.TransportOrders.FindAsync(id);
            if (order != null)
            {
                var cargoDTO = mapper.Map<TransportOrderDTO>(order);
                cache.Set(cacheKey, cargoDTO, TimeSpan.FromMinutes(360));
                return cargoDTO;
            }
            else
            {
                return null;
            }
        }
        public async Task<TransportOrder?> GetEntityAsync(int id)
        {
            if (id <= 0)
                throw new InvalidParameterException($"Передано некорректное значение ID: {id}");
            TransportOrder? order = await db.TransportOrders.FindAsync(id);
            if(order == null) 
                return null;
            return order;
        }
        public IEnumerable<TransportOrderDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new InvalidPaginationParameterException("Параметры page и pageSize должны быть больше нуля.");
            }
            var cacheKey = $"{CacheKeyPrefix}_V{CacheVersion}_Page{page}_Size{pageSize}";

            if (cache.TryGetValue(cacheKey, out IEnumerable<TransportOrderDTO>? cachedPage))
            {
                if (cachedPage != null)
                    return cachedPage;
            }
            var query = db.TransportOrders.Skip((page - 1) * pageSize).Take(pageSize);

            if (!query.Any())
            {
                throw new InvalidParameterException("Указанная страница не существует.");
            }
            var order = query.Select(order => new TransportOrderDTO
            {
                Id = order.Id,
                UserId = order.UserId,
                ShipperId = order.ShipperId,
                ConsigneeId = order.ConsigneeId,
                RouteId = order.RouteId,
                VehicleId = order.VehicleId,
                CreatedAt = order.CreatedAt,
                CompletionTime = order.CompletionTime,
                StartTransportTime = order.StartTransportTime,
                PlannedDeliveryTime = order.PlannedDeliveryTime,
                Notes = order.Notes,
                OrderStatus = order.OrderStatus,
                TrackNumber = order.TrackNumber
            }).ToList();
            cache.Set(cacheKey, order, TimeSpan.FromMinutes(360));
            return order;
        }

        public async Task<TransportOrder?> GetOrderInfoByTrackerAsync(string trackNumber)
        {
            if (string.IsNullOrEmpty(trackNumber))
            {
                throw new InvalidParameterException("Не указан трек-номер");
            }
            TransportOrder? transportOrder = await db.TransportOrders.FirstOrDefaultAsync(order => order.TrackNumber == trackNumber);
            return transportOrder;
        }

        public async Task<List<TransportOrder>> GetUserOrdersByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new InvalidParameterException($"Передано некорректное значение ID: {userId}");
            var orders = await db.TransportOrders.Where(order => order.UserId == userId).ToListAsync();
            return orders;
        }

        public async Task<List<TransportOrder>> GetPendingOrdersAsync()
        {
            var orders = await db.TransportOrders.Where(order => order.OrderStatus == OrderStatus.Pending).ToListAsync();
            return orders;
        }
        public async Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus)
        {
            var order = await db.TransportOrders.FindAsync(orderId);
            if (order == null)
                throw new EntityNotFoundException($"Заказ с ID {orderId} не найден.");
            order.OrderStatus = newStatus;
            if (newStatus == OrderStatus.Completed)
                order.CompletionTime = DateTime.UtcNow;
            else if(newStatus == OrderStatus.AcceptedByDriver)
                order.StartTransportTime = DateTime.UtcNow;
            await db.SaveChangesAsync();
        }
        public async Task<List<DriverOrderDTO>> GetOrdersForDriverAsync(int driverId)
        {
            var orders = await db.TransportOrders.Where(order => order.Vehicle != null && order.Vehicle.DriverId == driverId && order.OrderStatus != OrderStatus.Cancelled).ToListAsync();
            var ordersDTO =  orders.Select(order => mapper.Map<DriverOrderDTO>(order)).ToList();
            return ordersDTO;
        }
    }
}
