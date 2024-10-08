﻿using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class RouteRepository : IRepository<RouteDTO>
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private IMemoryCache cache;
        private const string CacheKeyPrefix = "route_";
        public RouteRepository(ShippingDbContext db, IMapper mapper, IMemoryCache cache)
        {
            this.db = db;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task<RouteDTO> AddAsync(RouteDTO entityDTO)
        {
            Models.Route route = mapper.Map<Models.Route>(entityDTO);
            await db.Routes.AddRangeAsync(route);
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix);
            return mapper.Map<RouteDTO>(route);
        }

        public async Task<RouteDTO> UpdateAsync(RouteDTO entityDTO)
        {
            Models.Route? route = await db.Routes.FindAsync(entityDTO.Id);
            if (route != null)
            {
                mapper.Map(entityDTO, route);
                await db.SaveChangesAsync();
                string cacheKey = CacheKeyPrefix + entityDTO.Id;
                cache.Set(cacheKey, entityDTO, TimeSpan.FromMinutes(360));
                return mapper.Map<RouteDTO>(route);
            }
            else
            {
                throw new ArgumentException($"Route with ID {entityDTO} not found.");
            }
        }

        public async Task DeleteAsync(int id)
        {
            Models.Route? route = await db.Routes.FindAsync(id);
            if (route != null)
            {
                db.Routes.Remove(route);
                await db.SaveChangesAsync();
                cache.Remove(CacheKeyPrefix + id);
                cache.Remove(CacheKeyPrefix);
            }
            else
            {
                throw new ArgumentException($"Route with ID {id} not found.");
            }
        }

        public async Task<RouteDTO> GetAsync(int id)
        {
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out RouteDTO? cacheResult))
            {
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            Models.Route? route = await db.Routes.FindAsync(id);
            if (route != null)
            {
                var routeDTO = mapper.Map<RouteDTO>(route);
                cache.Set(cacheKey, routeDTO, TimeSpan.FromMinutes(360));
                return routeDTO;
            }
            else
            {
                throw new ArgumentException($"Route with ID {id} not found.");
            }
        }

        public IEnumerable<RouteDTO> GetAll()
        {
            if (cache.TryGetValue(CacheKeyPrefix, out IEnumerable<RouteDTO>? cacheList))
            {
                if (cacheList != null)
                {
                    return cacheList;
                }
            }
            var route = db.Routes.Select(x => mapper.Map<RouteDTO>(x)).ToList();
            cache.Set(CacheKeyPrefix, route, TimeSpan.FromMinutes(360));
            return route;
        }
    }
}
