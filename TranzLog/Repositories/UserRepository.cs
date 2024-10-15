using AutoMapper;
using TranzLog.Data;
using TranzLog.Interfaces;

namespace TranzLog.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        public UserRepository(ShippingDbContext context, IMapper mapper) 
        {
            db = context;
            this.mapper = mapper;
        }
    }
}
