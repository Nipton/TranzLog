using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserOrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        private readonly ILogger<UserOrderController> logger;
        public UserOrderController(IOrderService orderService, ILogger<UserOrderController> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<ActionResult<string>> CreateOrder(UserOrderDTO userOrderDTO)
        {
            try
            {
                var orderId = await orderService.CreateOrder(userOrderDTO, HttpContext);
                return Ok(orderId);
            }
            catch(ArgumentException ex) 
            {
                logger.LogError(ex.Message);
                return BadRequest("Неполные данные для создания заказа.");
            }
            catch(UnauthorizedAccessException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(401, "Ошибка аутентификации.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet("tracker-search")]
        public async Task<ActionResult<UserOrderResponseDTO>> GetOrderByTracker(string trackNumber)
        {
            if (string.IsNullOrEmpty(trackNumber))
                return BadRequest("Не указан трек-номер");
            try
            {
                var order = await orderService.GetOrderInfoByTrackerAsync(trackNumber);
                if (order != null)
                    return Ok(order);
                return BadRequest("Указанный трек-номер не найден.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet]
        public async Task<ActionResult<List<UserOrderResponseDTO>>> GetAllUserOrders()
        {
            try
            {
                var orders = await orderService.GetUserOrdersAsync(HttpContext);
                return Ok(orders);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(401, "Ошибка аутентификации.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpPatch]
        public async Task<ActionResult> CancelOrderAsync(int orderId)
        {
            try
            {
                await orderService.CancelOrderAsync(orderId, HttpContext);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                logger.LogError(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
