using AutoMapper;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class CargoRepository : IRepository<CargoDTO>
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private IMemoryCache cache;
        private const string CacheKeyPrefix = "cargo_";
        public CargoRepository(ShippingDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }
        private async Task ValidateRelatedEntitiesAsync(CargoDTO entityDTO)
        {
            if (entityDTO.TransportOrderId != null)
            {
                if (!await db.TransportOrders.AnyAsync(x => x.Id == entityDTO.TransportOrderId))
                    throw new EntityNotFoundException($"Заказ с ID {entityDTO.TransportOrderId} не найден.");
            }
        }
        public async Task<CargoDTO> AddAsync(CargoDTO entityDTO)
        {
            await ValidateRelatedEntitiesAsync(entityDTO);
            Cargo cargo = mapper.Map<Cargo>(entityDTO);
            await db.Cargo.AddRangeAsync(cargo);
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix);
            return mapper.Map<CargoDTO>(cargo);
        }

        public async Task<CargoDTO> UpdateAsync(CargoDTO entityDTO)
        {
            await ValidateRelatedEntitiesAsync(entityDTO);
            Cargo? cargo = await db.Cargo.FindAsync(entityDTO.Id);
            if (cargo != null)
            {
                mapper.Map(entityDTO, cargo);
                await db.SaveChangesAsync();
                string cacheKey = CacheKeyPrefix + entityDTO.Id;
                cache.Set(cacheKey, entityDTO, TimeSpan.FromMinutes(360));
                return mapper.Map<CargoDTO>(cargo);
            }
            else
            {
                throw new ArgumentException($"Cargo with ID {entityDTO} not found.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            Cargo? cargo = await db.Cargo.FindAsync(id);
            if (cargo != null)
            {
                db.Cargo.Remove(cargo);
                await db.SaveChangesAsync();
                cache.Remove(CacheKeyPrefix + id);
                cache.Remove(CacheKeyPrefix);
            }
            else
            {
                throw new ArgumentException($"Cargo with ID {id} not found.");
            }
        }

        public async Task<CargoDTO?> GetAsync(int id)
        {
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out CargoDTO? cacheResult))
            {
                if(cacheResult != null)
                {
                    return cacheResult;
                }
            }
            Cargo? cargo = await db.Cargo.FindAsync(id);
            if (cargo != null)
            {
                var cargoDTO = mapper.Map<CargoDTO>(cargo);
                cache.Set(cacheKey, cargoDTO, TimeSpan.FromMinutes(360));
                return cargoDTO;
            }
            else
            {
                return null;
            }
        }

        public IEnumerable<CargoDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new ArgumentException("Параметры page и pageSize должны быть больше нуля.");
            }
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<CargoDTO>? cacheList))
            {
                if(cacheList != null)
                {
                    return cacheList.Skip((page - 1) * pageSize).Take(pageSize);
                }
            }
            var cargo = db.Cargo.Skip((page - 1) * pageSize).Take(pageSize).Select(x => mapper.Map<CargoDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, cargo, TimeSpan.FromMinutes(360));
            return cargo;
        }
    }
}
