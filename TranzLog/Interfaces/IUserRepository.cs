using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IUserRepository
    {
        Task AddUserAsync(User user);
        Task<UserDTO?> GetUserByIdAsync(int id);
        Task<UserDTO?> GetUserByNameAsync(string userName);
        IEnumerable<UserDTO> GetAllUsers();
        Task<UserDTO?> UpdateUserAsync(UserDTO userDTO);
        Task DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string name);
    }
}
