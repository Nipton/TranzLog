using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IManagerOrderService
    {
        Task<List<UserOrderResponseDTO>> GetPendingOrdersAsync();
        Task UpdateOrderStatusAsync(int orderId, int newStatus);
        Task ConfirmOrderAsync(ConfirmOrderRequestDTO request);
        Task UpdateDeliveryCost(int orderId);
    }
}
