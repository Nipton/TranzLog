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
    public class UserRepository : IUserRepository
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private IMemoryCache cache;
        private const string CacheKeyPrefix = "users_";
        public UserRepository(ShippingDbContext context, IMapper mapper, IMemoryCache cache) 
        {
            db = context;
            this.mapper = mapper;
            this.cache = cache;
        }

        public async Task AddUserAsync(User user)
        {
            await db.AddAsync(user);
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix);
        }

        public async Task<UserDTO?> GetUserByIdAsync(int id)
        {
            string cacheKey = CacheKeyPrefix + id;
            if (cache.TryGetValue(cacheKey, out UserDTO? cacheResult))
            {
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            User? user = await db.Users.FindAsync(id);
            if (user == null)
            {
                return null;
            }
            var userDTO = mapper.Map<UserDTO>(user);
            cache.Set(cacheKey, userDTO, TimeSpan.FromMinutes(360));
            return userDTO;
        }

        public async Task<UserDTO?> GetUserByNameAsync(string userName)
        {
            string cacheKey = CacheKeyPrefix + userName;
            if (cache.TryGetValue(cacheKey, out UserDTO? cacheReesult))
            {
                if (cacheReesult != null)
                {
                    return cacheReesult;
                }
            }
            User? user = await db.Users.FirstOrDefaultAsync(x => x.UserName == userName);
            if (user == null)
            {
                return null;
            }
            var userDTO = mapper.Map<UserDTO>(user);
            cache.Set(cacheKey, userDTO, TimeSpan.FromMinutes(360));
            return userDTO;
        }

        public async Task<UserDTO?> UpdateUserAsync(UserDTO userDTO)
        {
            User? user = await db.Users.FindAsync(userDTO.Id);
            if (user == null)
            {
                throw new UserNotFoundException($"Пользователь с ID {userDTO.Id} не найден.");
            }
            mapper.Map(userDTO, user); 
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix + userDTO.UserName);
            cache.Remove(CacheKeyPrefix + userDTO.Id);
            cache.Remove(CacheKeyPrefix);
            return mapper.Map<UserDTO>(user);
        }

        public async Task DeleteUserAsync(int id)
        {
            User? user = await db.Users.FindAsync(id);
            if(user == null)
            {
                throw new UserNotFoundException($"Пользователь с ID {id} не найден.");
            }
            db.Users.Remove(user);
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix + id);
            cache.Remove(CacheKeyPrefix);
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            string cacheKey = CacheKeyPrefix + userName;
            if (cache.TryGetValue(cacheKey, out UserDTO? cacheReesult))
            {
                if (cacheReesult != null)
                {
                    return true;
                }
            }
            User? user = await db.Users.FirstOrDefaultAsync(x=> x.UserName == userName);
            if (user != null)
            {
                UserDTO userDTO = mapper.Map<UserDTO>(user);
                cache.Set(cacheKey, userDTO, TimeSpan.FromMinutes(360));
                return true;
            }
            return false;
        }

        public IEnumerable<UserDTO> GetAllUsers()
        {
            if(cache.TryGetValue(CacheKeyPrefix, out IEnumerable<UserDTO>? cacheList))
            {
                if(cacheList != null)
                {
                    return cacheList;
                }
            }
            var users = db.Users.Select(user => new UserDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Role = user.Role,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CreatedDate = user.CreatedDate
            }).ToList();
            cache.Set(CacheKeyPrefix, users, TimeSpan.FromMinutes(360));
            return users;
        }
    }
}
