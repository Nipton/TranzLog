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
            cache.Remove(CacheKeyPrefix);
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
                return mapper.Map<TransportOrderDTO>(order);
            }
            else
            {
                throw new InvalidParameterException($"Order with ID {entityDTO} not found.");
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
                cache.Remove(CacheKeyPrefix);
            }
            else
            {
                throw new ArgumentException($"Order with ID {id} not found.");
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

        public IEnumerable<TransportOrderDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new ArgumentException("Параметры page и pageSize должны быть больше нуля.");
            }
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<TransportOrderDTO>? cacheList))
            {
                if (cacheList != null)
                {
                    return cacheList.Skip((page - 1) * pageSize).Take(pageSize);
                }
            }
            var order = db.TransportOrders.Skip((page - 1) * pageSize).Take(pageSize).Select(x => mapper.Map<TransportOrderDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, order, TimeSpan.FromMinutes(360));
            return order;
        }

        public async Task<TransportOrder?> GetOrderInfoByTrackerAsync(string trackNumber)
        {
            if (string.IsNullOrEmpty(trackNumber))
            {
                throw new ArgumentException("Не указан трек-номер");
            }
            TransportOrder? transportOrder = await db.TransportOrders.FirstOrDefaultAsync(order => order.TrackNumber == trackNumber);
            return transportOrder;
        }

        public async Task<List<TransportOrder>> GetUserOrdersByIdAsync(int userId)
        {
            if (userId <= 0)
                throw new ArgumentException($"Передано некорректное значение ID: {userId}");
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
                throw new ArgumentException($"Заказ с ID {orderId} не найден.");
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
