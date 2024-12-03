using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<ActionResult<string>> CreateOrder(UserOrderRequestDTO userOrderDTO)
        {
            try
            {
                var orderId = await orderService.CreateOrderByUserAsync(userOrderDTO, HttpContext);
                return Ok(orderId);
            }
            catch(InvalidParameterException ex) 
            {
                logger.LogWarning(ex, ex.Message);
                return BadRequest("Неполные данные для создания заказа.");
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return NotFound($"{ex.Message}");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(401, "Ошибка аутентификации.");
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        [HttpGet("tracker-search")]
        [AllowAnonymous]
        public async Task<ActionResult<UserOrderResponseDTO>> GetOrderByTracker(string trackNumber)
        {
            if (string.IsNullOrEmpty(trackNumber))
                return BadRequest("Не указан трек-номер");
            try
            {
                var order = await orderService.GetOrderInfoByTrackerAsync(trackNumber);
                if (order != null)
                    return Ok(order);
                return NotFound("Указанный трек-номер не найден.");
            }
            catch (InvalidParameterException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return BadRequest($"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
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
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
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
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(403, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
