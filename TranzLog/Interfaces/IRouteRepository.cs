using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IRouteRepository : IRepository<RouteDTO>
    {
        Task<RouteDTO?> GetRoutesAsync(string from, string to);
        Task<bool> RouteExistsAsync(int id);
    }
}
