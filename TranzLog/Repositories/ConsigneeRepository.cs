﻿using AutoMapper;
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
            cache.Remove(CacheKeyPrefix);
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
                cache.Remove(CacheKeyPrefix);
            }
            else
            {
                throw new EntityNotFoundException($"Consignee with ID {id} not found.");
            }          
        }

        public IEnumerable<ConsigneeDTO> GetAll(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new InvalidPaginationParameterException("Параметры page и pageSize должны быть больше нуля.");
            }
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<ConsigneeDTO>? result))
            {
                if(result != null)
                    return result.Skip((page - 1) * pageSize).Take(pageSize);
            }
            var list = db.Consignees.Skip((page - 1) * pageSize).Take(pageSize).Select(x => mapper.Map<ConsigneeDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, list, TimeSpan.FromMinutes(360));
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
                return mapper.Map<ConsigneeDTO>(consignee);
            }
            else
            {
                throw new EntityNotFoundException($"Consignee with ID {entityDTO.Id} not found.");
            }
        }
    }
}
