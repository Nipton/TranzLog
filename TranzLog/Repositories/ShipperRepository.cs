using AutoMapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
using TranzLog.Exceptions;
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
        private static int CacheVersion = 0;
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
            Interlocked.Increment(ref CacheVersion);
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
                Interlocked.Increment(ref CacheVersion);
            }
            else
            {
                throw new EntityNotFoundException($"Отправитель с ID {id} не найден.");
            }           
        }

        public IEnumerable<ShipperDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new InvalidPaginationParameterException("Параметры page и pageSize должны быть больше нуля.");
            }
            var cacheKey = $"{CacheKeyPrefix}_V{CacheVersion}_Page{page}_Size{pageSize}";

            if (cache.TryGetValue(cacheKey, out IEnumerable<ShipperDTO>? cachedPage))
            {
                if(cachedPage != null)
                    return cachedPage;
            }
            var query = db.Shippers.Skip((page - 1) * pageSize).Take(pageSize);

            if (!query.Any())
            {
                throw new InvalidParameterException("Указанная страница не существует.");
            }
            var shippers = query.Select(x => mapper.Map<ShipperDTO>(x)).ToList();
            cache.Set(cacheKey, shippers, TimeSpan.FromMinutes(360));
            return shippers;
        }

        public async Task<ShipperDTO?> GetAsync(int id)
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
                return null;
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
                Interlocked.Increment(ref CacheVersion);
                return mapper.Map<ShipperDTO>(shipper);               
            }
            else
            {
                throw new EntityNotFoundException($"Shipper with ID {entity.Id} not found.");
            }           
        }
    }
}
