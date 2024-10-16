using AutoMapper;
using TranzLog.Data;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Repositories
{
    public class UserRepository : IRepository<UserDTO>
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        public UserRepository(ShippingDbContext context, IMapper mapper) 
        {
            db = context;
            this.mapper = mapper;
        }

        public Task<UserDTO> AddAsync(UserDTO entityDTO)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<UserDTO> GetAll()
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> GetAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<UserDTO> UpdateAsync(UserDTO entityDTO)
        {
            throw new NotImplementedException();
        }
    }
}
