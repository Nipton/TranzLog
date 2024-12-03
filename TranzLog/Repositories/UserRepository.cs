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
    public class UserRepository : IUserRepository
    {
        private readonly ShippingDbContext db;
        private readonly IMapper mapper;
        private IMemoryCache cache;
        private const string CacheKeyPrefix = "users_";
        private static int CacheVersion = 0;
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
            Interlocked.Increment(ref CacheVersion);
        }
        public async Task UpdateUserAsync(User user)
        {
            db.Update(user);
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix + user.Id);
            Interlocked.Increment(ref CacheVersion);
        }
        public async Task<User?> GetUserEntityByNameAsync(string userName)
        {
            User? user = await db.Users.SingleOrDefaultAsync(x => x.UserName == userName);
            return user;
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
            if (cache.TryGetValue(cacheKey, out UserDTO? cacheResult))
            {
                if (cacheResult != null)
                {
                    return cacheResult;
                }
            }
            User? user = await db.Users.SingleOrDefaultAsync(x => x.UserName == userName);
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
            User? userByUserName = await db.Users.SingleOrDefaultAsync(x => x.UserName == userDTO.UserName);
            if (userByUserName != null && userByUserName.Id != user.Id)
            {
                throw new DuplicateException($"Имя пользователя {userByUserName.UserName} уже занято");   
            }
            user.FirstName = userDTO.FirstName;
            user.LastName = userDTO.LastName;
            user.Email = userDTO.Email;
            user.PhoneNumber = userDTO.PhoneNumber;
            user.UserName = userDTO.UserName;
            await db.SaveChangesAsync();
            cache.Remove(CacheKeyPrefix + userDTO.UserName);
            cache.Remove(CacheKeyPrefix + userDTO.Id);
            Interlocked.Increment(ref CacheVersion);
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
            Interlocked.Increment(ref CacheVersion);
        }

        public async Task<bool> UserExistsAsync(string userName)
        {
            string cacheKey = CacheKeyPrefix + userName;
            if (cache.TryGetValue(cacheKey, out UserDTO? cacheResult))
            {
                if (cacheResult != null)
                {
                    return true;
                }
            }
            User? user = await db.Users.SingleOrDefaultAsync(x=> x.UserName == userName);
            if (user != null)
            {
                UserDTO userDTO = mapper.Map<UserDTO>(user);
                cache.Set(cacheKey, userDTO, TimeSpan.FromMinutes(360));
                return true;
            }
            return false;
        }

        public IEnumerable<UserDTO> GetAllUsers(int page = 1, int pageSize = 10)
        {
            if (page < 1 || pageSize < 1)
            {
                throw new InvalidPaginationParameterException("Параметры page и pageSize должны быть больше нуля.");
            }
            var cacheKey = $"{CacheKeyPrefix}_V{CacheVersion}_Page{page}_Size{pageSize}";

            if (cache.TryGetValue(cacheKey, out IEnumerable<UserDTO>? cachedPage))
            {
                if (cachedPage != null)
                    return cachedPage;
            }
            var query = db.Users.Skip((page - 1) * pageSize).Take(pageSize);

            if (!query.Any())
            {
                throw new InvalidParameterException("Указанная страница не существует.");
            }
            var users = query.Select(user => new UserDTO
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
            cache.Set(cacheKey, users, TimeSpan.FromMinutes(360));
            return users;
        }      
    }
}