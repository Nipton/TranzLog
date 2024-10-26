using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IDriverRepository : IRepository<DriverDTO>
    {
        Task<Driver?> FindDriverByUserIdAsync(int userId);
    }
}
