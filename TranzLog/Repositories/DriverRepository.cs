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
    public class DriverRepository : IDriverRepository
    {        
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private const string CacheKeyPrefix = "drivers_";
        private static int CacheVersion = 0;
        public DriverRepository(ShippingDbContext shippingDbContext, IMemoryCache cache, IMapper mapper) 
        {
            db = shippingDbContext;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<DriverDTO> AddAsync(DriverDTO entityDTO)
        {
            Driver driver = mapper.Map<Driver>(entityDTO);
            if (driver.UserId != null)
            {
                var user = await db.Users.FindAsync(driver.UserId);
                if (user != null)
                {
                    if(user.Role == Role.User)
                        user.Role = Role.Driver;
                }
                else
                {
                    driver.UserId = null;
                }
            }
            await db.Drivers.AddAsync(driver);
            await db.SaveChangesAsync();
            Interlocked.Increment(ref CacheVersion);
            return mapper.Map<DriverDTO>(driver);
        }
        public async Task DeleteAsync(int id)
        {
            Driver? driver = await db.Drivers.FindAsync(id);
            if (driver != null)
            {
                db.Drivers.Remove(driver);
                await db.SaveChangesAsync();
                cache.Remove(CacheKeyPrefix + id);
                Interlocked.Increment(ref CacheVersion);
            }
            else
            {
                throw new EntityNotFoundException($"Водитель с ID {id} не  найден.");
            }
        }

        public IEnumerable<DriverDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new InvalidPaginationParameterException("Параметры page и pageSize должны быть больше нуля.");
            }
            var cacheKey = $"{CacheKeyPrefix}_V{CacheVersion}_Page{page}_Size{pageSize}";

            if (cache.TryGetValue(cacheKey, out IEnumerable<DriverDTO>? cachedPage))
            {
                if (cachedPage != null)
                    return cachedPage;
            }
            var query = db.Drivers.Skip((page - 1) * pageSize).Take(pageSize);

            if (!query.Any())
            {
                throw new InvalidParameterException("Указанная страница не существует.");
            }
            var drivers = query.Select(x => mapper.Map<DriverDTO>(x)).ToList();
            cache.Set(cacheKey, drivers, TimeSpan.FromMinutes(360));
            return drivers;
        }

        public async Task<DriverDTO?> GetAsync(int id)
        {
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out DriverDTO? cachDriver))
            {
                if (cachDriver != null)
                {
                    return cachDriver;
                }
            }
            Driver? driver = await db.Drivers.FindAsync(id);
            if (driver != null)
            {
                var driverDTO = mapper.Map<DriverDTO>(driver);
                cache.Set(cacheKey, driverDTO, TimeSpan.FromMinutes(360));
                return driverDTO;
            }
            else
            {
                return null;
            }
        }

        public async Task<DriverDTO> UpdateAsync(DriverDTO entityDTO)
        {
            Driver? driver = await db.Drivers.FindAsync(entityDTO.Id);
            if (driver == null)
                throw new EntityNotFoundException($"Водитель с ID {entityDTO.Id} не  найден.");
            if (driver.UserId != null)
            {
                var user = await db.Users.FindAsync(driver.UserId);
                if (user != null)
                {
                    if (user.Role == Role.User)
                        user.Role = Role.Driver;
                }
                else
                {
                    driver.UserId = null;
                }
            }
            mapper.Map(entityDTO, driver);
            await db.SaveChangesAsync();
            string cacheKey = CacheKeyPrefix + entityDTO.Id;
            cache.Set(cacheKey, entityDTO, TimeSpan.FromMinutes(360));
            Interlocked.Increment(ref CacheVersion);
            return mapper.Map<DriverDTO>(driver);
        }

        public async Task<Driver?> FindDriverByUserIdAsync(int driverUserId)
        {
            var driver = await db.Drivers.FirstOrDefaultAsync(driver => driver.UserId == driverUserId);
            return driver;
        }
    }
}
