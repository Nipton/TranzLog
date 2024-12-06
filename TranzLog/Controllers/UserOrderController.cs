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
        /// <summary>
        /// Создать новый заказ.
        /// </summary>
        /// <param name="userOrderDTO">Данные для создания заказа.</param>
        /// <returns>ID созданного заказа.</returns>
        /// <response code="200">Заказ успешно создан.</response>
        /// <response code="400">Неполные или некорректные данные.</response>
        /// <response code="401">Ошибка аутентификации.</response>
        /// <response code="404">Связанная сущность не найдена.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Получить информацию о заказе по трек-номеру.
        /// </summary>
        /// <param name="trackNumber">Трек-номер заказа.</param>
        /// <returns>Информация о заказе.</returns>
        /// <response code="200">Заказ успешно найден.</response>
        /// <response code="400">Некорректный или пустой трек-номер.</response>
        /// <response code="404">Заказ с указанным трек-номером не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("tracker-search")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Получить все заказы текущего пользователя.
        /// </summary>
        /// <returns>Список заказов пользователя.</returns>
        /// <response code="200">Список заказов успешно получен.</response>
        /// <response code="401">Ошибка аутентификации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Отменить заказ.
        /// </summary>
        /// <param name="orderId">ID заказа.</param>
        /// <response code="200">Заказ успешно отменён.</response>
        /// <response code="403">Недостаточно прав для выполнения операции.</response>
        /// <response code="404">Заказ не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
