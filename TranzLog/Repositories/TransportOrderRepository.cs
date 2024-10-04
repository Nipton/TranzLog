using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class TransportOrderRepository : IRepository<TransportOrderDTO>
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
            await db.TransportOrders.AddRangeAsync(order);
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
                throw new ArgumentException($"Order with ID {entityDTO} not found.");
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

        public async Task<TransportOrderDTO> GetAsync(int id)
        {
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
                throw new ArgumentException($"Order with ID {id} not found.");
            }
        }

        public IEnumerable<TransportOrderDTO> GetAll()
        {
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<TransportOrderDTO>? cacheList))
            {
                if (cacheList != null)
                {
                    return cacheList;
                }
            }
            var order = db.TransportOrders.Select(x => mapper.Map<TransportOrderDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, order, TimeSpan.FromMinutes(360));
            return order;
        }
    }
}
