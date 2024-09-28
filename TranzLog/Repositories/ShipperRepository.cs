using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class ShipperRepository : IRepository<ShipperDTO>
    {
        private readonly ShipperRepository db;
        private readonly IMapper mapper;
        private readonly IMemoryCache cashe;
        public ShipperRepository(ShipperRepository db, IMapper mapper, IMemoryCache cache) 
        {
            this.db = db;
            this.mapper = mapper;
            this.cashe = cache;
        }
    }
}
