using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class ConsigneeRepository : IRepository<ConsigneeDTO>
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cache;
        private const string CacheKeyPrefix = "consignee_";
        private static int CacheVersion = 0;
        public ConsigneeRepository(ShippingDbContext db, IMapper mapper, IMemoryCache cache) 
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<ConsigneeDTO> AddAsync(ConsigneeDTO entityDTO)
        {
            Consignee consignee = mapper.Map<Consignee>(entityDTO);
            await db.Consignees.AddAsync(consignee);
            await db.SaveChangesAsync();
            Interlocked.Increment(ref CacheVersion);
            return mapper.Map<ConsigneeDTO>(consignee);
        }

        public async Task DeleteAsync(int id)
        {
            Consignee? consignee = await db.Consignees.FindAsync(id);
            if (consignee != null)
            {
                db.Consignees.Remove(consignee);
                await db.SaveChangesAsync();
                cache.Remove(CacheKeyPrefix + id);
                Interlocked.Increment(ref CacheVersion);
            }
            else
            {
                throw new EntityNotFoundException($"Получатель с ID {id} не найден.");
            }          
        }

        public IEnumerable<ConsigneeDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new InvalidPaginationParameterException("Параметры page и pageSize должны быть больше нуля.");
            }
            var cacheKey = $"{CacheKeyPrefix}_V{CacheVersion}_Page{page}_Size{pageSize}";

            if (cache.TryGetValue(cacheKey, out IEnumerable<ConsigneeDTO>? cachedPage))
            {
                if (cachedPage != null)
                    return cachedPage;
            }
            var query = db.Consignees.Skip((page - 1) * pageSize).Take(pageSize);

            if (!query.Any())
            {
                throw new InvalidParameterException("Указанная страница не существует.");
            }
            var list = query.Select(x => mapper.Map<ConsigneeDTO>(x)).ToList();
            cache.Set(cacheKey, list, TimeSpan.FromMinutes(360));
            return list;
        }

        public async Task<ConsigneeDTO?> GetAsync(int id)
        {
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out ConsigneeDTO? cacheConsignee))
            {
                if(cacheConsignee != null)
                {
                    return cacheConsignee;
                }
            }
            Consignee? consignee = await db.Consignees.FindAsync(id);
            if(consignee != null)
            {
                var consigneeDTO = mapper.Map<ConsigneeDTO>(consignee);
                cache.Set(cacheKey, consigneeDTO, TimeSpan.FromMinutes(360));
                return consigneeDTO;
            }
            else
            {
                return null;
            }
        }

        public async Task<ConsigneeDTO> UpdateAsync(ConsigneeDTO entityDTO)
        {
            Consignee? consignee = await db.Consignees.FindAsync(entityDTO.Id);
            if (consignee != null)
            {
                mapper.Map(entityDTO, consignee);
                await db.SaveChangesAsync();
                string cacheKey = CacheKeyPrefix + entityDTO.Id;
                cache.Set(cacheKey, entityDTO, TimeSpan.FromMinutes(360));
                Interlocked.Increment(ref CacheVersion);
                return mapper.Map<ConsigneeDTO>(consignee);
            }
            else
            {
                throw new EntityNotFoundException($"Получатель с ID {entityDTO.Id} не найден.");
            }
        }
    }
}
