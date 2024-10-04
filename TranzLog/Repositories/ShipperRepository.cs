using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class ShipperRepository : IRepository<ShipperDTO>
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private const string CacheKeyPrefix = "shippers_";
        public ShipperRepository(ShippingDbContext db, IMapper mapper, IMemoryCache cache) 
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<ShipperDTO> AddAsync(ShipperDTO shipperDTO)
        {
            Shipper shipper = mapper.Map<Shipper>(shipperDTO);
            await db.Shippers.AddAsync(shipper);
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix);
            return mapper.Map<ShipperDTO>(shipper);
        }

        public async Task DeleteAsync(int id)
        {
            Shipper? shipper = await db.Shippers.FindAsync(id);
            if (shipper != null)
            {
                db.Shippers.Remove(shipper);
                await db.SaveChangesAsync();
                cache.Remove(CacheKeyPrefix + id);
                cache.Remove(CacheKeyPrefix);
            }
            else
            {
                throw new ArgumentException($"Shipper with ID {id} not found.");
            }           
        }

        public IEnumerable<ShipperDTO> GetAll()
        {
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<ShipperDTO>? cacheList))
            {
                if(cacheList != null)
                    return cacheList;
            }
            var shippers = db.Shippers.Select(x => mapper.Map<ShipperDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, shippers, TimeSpan.FromMinutes(360));
            return shippers;
        }

        public async Task<ShipperDTO> GetAsync(int id)
        {
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out ShipperDTO? cacheResult))
            {
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            var shipper = await db.Shippers.FindAsync(id);
            if (shipper != null)
            {
                var shipperDTO = mapper.Map<ShipperDTO>(shipper);
                cache.Set(cacheKey, shipperDTO, TimeSpan.FromMinutes(360));
                return shipperDTO;
            }
            else
            {
                throw new ArgumentException($"Shipper with ID {id} not found.");
            }
        }

        public async Task<ShipperDTO> UpdateAsync(ShipperDTO entity)
        {
            Shipper? shipper = await db.Shippers.FindAsync(entity.Id);
            if (shipper != null)
            {
                mapper.Map(entity, shipper);
                await db.SaveChangesAsync();
                string cacheKey = CacheKeyPrefix + entity.Id;
                cache.Set(cacheKey, entity, TimeSpan.FromMinutes(360));
                return mapper.Map<ShipperDTO>(shipper);               
            }
            else
            {
                throw new ArgumentException($"Shipper with ID {entity.Id} not found.");
            }           
        }
    }
}
