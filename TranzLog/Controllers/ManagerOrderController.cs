using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ManagerOrderController : ControllerBase
    {
        private readonly IManagerOrderService managerOrderService;
        private readonly ILogger<ManagerOrderController> logger;
        public ManagerOrderController(IManagerOrderService managerOrderService, ILogger<ManagerOrderController> logger)
        {
            this.managerOrderService = managerOrderService;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<List<UserOrderResponseDTO>>> GetPendingOrders()
        {
            try
            {
                var orders = await managerOrderService.GetPendingOrdersAsync();
                return Ok(orders);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpPatch("/api/orders/{orderId}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int orderId, int newStatus)
        {
            try
            {
                await managerOrderService.UpdateOrderStatusAsync(orderId, newStatus);
                return Ok("Статус заказа успешно обновлен.");
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
