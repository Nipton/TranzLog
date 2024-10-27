using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator, Manager")]
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
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        [HttpPatch("{orderId}/status")]
        public async Task<ActionResult> UpdateOrderStatus(int orderId, int newStatus)
        {
            try
            {
                await managerOrderService.UpdateOrderStatusAsync(orderId, newStatus);
                return Ok("Статус заказа успешно обновлен.");
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return NotFound($"{ex.Message}");
            }
            catch (InvalidParameterException ex)
            {
                logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
