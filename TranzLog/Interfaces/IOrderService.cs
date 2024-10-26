using Microsoft.AspNetCore.Mvc;
using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IOrderService
    {
        Task<string> CreateOrderByUserAsync(UserOrderDTO userOrderDTO, HttpContext httpContext);
        Task<UserOrderResponseDTO?> GetOrderInfoByTrackerAsync(string trackNumber);
        Task<List<UserOrderResponseDTO>> GetUserOrdersAsync(HttpContext httpContext);
        Task CancelOrderAsync(int orderId, HttpContext httpContext);
        Task<TransportOrderDTO> CreateOrderAsync(TransportOrderDTO orderDTO);
        Task<TransportOrderDTO> UpdateOrderAsync(TransportOrderDTO orderDTO);
        Task<ActionResult<TransportOrderDTO?>> GetOrderAsync(int id);
        IEnumerable<TransportOrderDTO> GetAll(int page, int pageSize);
        Task DeleteAsync(int id);
    }
}
