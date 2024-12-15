using System.Threading.Tasks;
using TranzLog.Models;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

namespace TranzLog.Interfaces
{
    public interface ITransportOrderRepository : IRepository<TransportOrderDTO>
    {
        Task<TransportOrder?> GetOrderInfoByTrackerAsync(string trackNumber);
        Task<List<TransportOrder>> GetUserOrdersByIdAsync(int userId);
        Task<List<TransportOrder>> GetPendingOrdersAsync();
        Task UpdateOrderStatusAsync(int orderId, OrderStatus newStatus);
        Task<List<DriverOrderDTO>> GetOrdersForDriverAsync(int driverId);
        Task<TransportOrder?> GetEntityAsync(int id);
    }
}
