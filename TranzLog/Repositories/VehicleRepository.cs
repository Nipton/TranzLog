using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
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

        public async Task<VehicleDTO> AddAsync(VehicleDTO entityDTO)
        {
            Vehicle vehicle = mapper.Map<Vehicle>(entityDTO);
            await db.Vehicles.AddRangeAsync(vehicle);
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix);
            return mapper.Map<VehicleDTO>(vehicle);
        }

        public async Task<VehicleDTO> UpdateAsync(VehicleDTO entityDTO)
        {
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

        public async Task<VehicleDTO> GetAsync(int id)
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
                throw new ArgumentException($"Vehicle with ID {id} not found.");
            }
        }

        public IEnumerable<VehicleDTO> GetAll()
        {
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<VehicleDTO>? cacheList))
            {
                if (cacheList != null)
                {
                    return cacheList;
                }
            }
            var vehicle = db.Vehicles.Select(x => mapper.Map<VehicleDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, vehicle, TimeSpan.FromMinutes(360));
            return vehicle;
        }
    }
}
