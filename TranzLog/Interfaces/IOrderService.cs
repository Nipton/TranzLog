using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IOrderService
    {
        Task<string> CreateOrder(UserOrderDTO userOrderDTO, HttpContext httpContext);
        Task<UserOrderResponseDTO?> GetOrderInfoByTrackerAsync(string trackNumber);
        Task<List<UserOrderResponseDTO>> GetUserOrdersAsync(HttpContext httpContext);
        Task CancelOrderAsync(int orderId, HttpContext httpContext);
    }
}
