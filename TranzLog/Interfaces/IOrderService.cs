using TranzLog.Models.DTO;

namespace TranzLog.Interfaces
{
    public interface IOrderService
    {
        Task<int> CreateOrder(UserOrderDTO userOrderDTO, HttpContext httpContext);
    }
}
