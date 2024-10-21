using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<User?> GetUserEntityByNameAsync(string userName);
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<UserDTO?> GetUserByNameAsync(string userName);
        IEnumerable<UserDTO> GetAllUsers();
        Task<UserDTO?> UpdateUserAsync(UserDTO userDTO);
        Task UpdateUserAsync(User user);
        Task DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string name);
    }
}
