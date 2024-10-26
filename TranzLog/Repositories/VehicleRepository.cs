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
            cache.Remove(CacheKeyPrefix);
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
                return mapper.Map<VehicleDTO>(vehicle);
            }
            else
            {
                throw new ArgumentException($"Vehicle with ID {entityDTO} not found.");
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
                cache.Remove(CacheKeyPrefix);
            }
            else
            {
                throw new ArgumentException($"Vehicle with ID {id} not found.");
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
                throw new ArgumentException("Параметры page и pageSize должны быть больше нуля.");
            }
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<VehicleDTO>? cacheList))
            {
                if (cacheList != null)
                {
                    return cacheList.Skip((page - 1) * pageSize).Take(pageSize);
                }
            }
            var vehicle = db.Vehicles.Skip((page - 1) * pageSize).Take(pageSize).Select(x => mapper.Map<VehicleDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, vehicle, TimeSpan.FromMinutes(360));
            return vehicle;
        }
    }
}
