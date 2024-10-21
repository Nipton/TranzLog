using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Services
{
    public class OrderService
    {
        private readonly IUserRepository repo;
        public OrderService(IUserRepository repo)
        {
            this.repo = repo;
        }
        public UserOrderDTO CreateOrder(UserOrderDTO userOrderDTO, HttpContext httpContext)
        {
            throw new NotImplementedException();
        }
    }
}
