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
    public class VehicleRepository : IRepository<VehicleDTO>
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private IMemoryCache cache;
        private const string CacheKeyPrefix = "vehicle_";
        private static int CacheVersion = 0;

        public VehicleRepository(ShippingDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }
        private async Task ValidateRelatedEntitiesAsync(VehicleDTO entityDTO)
        {
            if (entityDTO.DriverId != null)
            {
                if (!await db.Drivers.AnyAsync(x => x.Id == entityDTO.DriverId))
                    throw new EntityNotFoundException($"Водитель с ID {entityDTO.DriverId} не найден.");
            }
        }
        public async Task<VehicleDTO> AddAsync(VehicleDTO entityDTO)
        {
            await ValidateRelatedEntitiesAsync(entityDTO);
            Vehicle vehicle = mapper.Map<Vehicle>(entityDTO);
            await db.Vehicles.AddRangeAsync(vehicle);
            await db.SaveChangesAsync();
            Interlocked.Increment(ref CacheVersion);
            return mapper.Map<VehicleDTO>(vehicle);
        }

        public async Task<VehicleDTO> UpdateAsync(VehicleDTO entityDTO)
        {
            await ValidateRelatedEntitiesAsync(entityDTO);
            Vehicle? vehicle = await db.Vehicles.FindAsync(entityDTO.Id);
            if (vehicle != null)
            {
                mapper.Map(entityDTO, vehicle);
                await db.SaveChangesAsync();
                string cacheKey = CacheKeyPrefix + entityDTO.Id;
                cache.Set(cacheKey, entityDTO, TimeSpan.FromMinutes(360));
                Interlocked.Increment(ref CacheVersion);
                return mapper.Map<VehicleDTO>(vehicle);
            }
            else
            {
                throw new EntityNotFoundException($"Транспорт с ID {entityDTO} не найден.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            Vehicle? vehicle = await db.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                db.Vehicles.Remove(vehicle);
                await db.SaveChangesAsync();
                cache.Remove(CacheKeyPrefix + id);
                Interlocked.Increment(ref CacheVersion);
            }
            else
            {
                throw new EntityNotFoundException($"Транспорт с ID {id} не найден.");
            }
        }

        public async Task<VehicleDTO?> GetAsync(int id)
        {
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out VehicleDTO? cacheResult))
            {
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            Vehicle? vehicle = await db.Vehicles.FindAsync(id);
            if (vehicle != null)
            {
                var vehicleDTO = mapper.Map<VehicleDTO>(vehicle);
                cache.Set(cacheKey, vehicleDTO, TimeSpan.FromMinutes(360));
                return vehicleDTO;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<VehicleDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new InvalidPaginationParameterException("Параметры page и pageSize должны быть больше нуля.");
            }
            var cacheKey = $"{CacheKeyPrefix}_V{CacheVersion}_Page{page}_Size{pageSize}";

            if (cache.TryGetValue(cacheKey, out IEnumerable<VehicleDTO>? cachedPage))
            {
                if (cachedPage != null)
                    return cachedPage;
            }
            var query = db.Vehicles.Skip((page - 1) * pageSize).Take(pageSize);

            if (!query.Any())
            {
                throw new InvalidParameterException("Указанная страница не существует.");
            }
            var vehicle = query.Select(x => mapper.Map<VehicleDTO>(x)).ToList();
            cache.Set(cacheKey, vehicle, TimeSpan.FromMinutes(360));
            return vehicle;
        }
    }
}
