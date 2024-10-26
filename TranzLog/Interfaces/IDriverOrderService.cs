using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IDriverOrderService
    {
        Task<List<DriverOrderDTO>> GetDriverAssignedOrdersAsync(HttpContext httpContext);
        Task UpdateOrderDeliveryStatusAsync(int orderId, int newStatus, HttpContext httpContext);
    }
}
