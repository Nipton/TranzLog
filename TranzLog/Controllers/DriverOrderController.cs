using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriverOrderController : ControllerBase
    {
        private readonly IDriverOrderService orderService;
        private readonly ILogger<DriverOrderController> logger;
        public DriverOrderController(IDriverOrderService orderService, ILogger<DriverOrderController> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<List<DriverOrderDTO>>> GetDriverAssignedOrders()
        {
            try
            {
                var orders = await orderService.GetDriverAssignedOrdersAsync(HttpContext);
                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(401, "Ошибка аутентификации.");
            }
            catch (UserNotFoundException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpPatch]
        public async Task<ActionResult> UpdateOrderDeliveryStatus(int orderId, int newStatus)
        {
            try
            {
                await orderService.UpdateOrderDeliveryStatusAsync(orderId, newStatus, HttpContext);
                return Ok("Статус успешно изменен.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(401, "Ошибка аутентификации.");
            }
            catch (AccessDeniedException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(403, ex.Message);
            }
            catch (ArgumentException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (UserNotFoundException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
