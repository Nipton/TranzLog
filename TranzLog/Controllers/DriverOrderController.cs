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
    [Authorize(Roles = "Administrator, Driver")]
    public class DriverOrderController : ControllerBase
    {
        private readonly IDriverOrderService orderService;
        private readonly ILogger<DriverOrderController> logger;
        public DriverOrderController(IDriverOrderService orderService, ILogger<DriverOrderController> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }
        /// <summary>
        /// Получить список заказов, назначенных водителю.
        /// </summary>
        /// <returns>Список назначенных заказов.</returns>
        /// <response code="200">Список успешно получен.</response>
        /// <response code="401">Ошибка аутентификации.</response>
        /// <response code="404">Пользователь не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Обновить статус доставки заказа.
        /// </summary>
        /// /// <remarks>
        /// Водителю статус заказа можно изменить только на 'AcceptedByDriver' или 'Completed'.
        /// </remarks>
        /// <param name="orderId">ID заказа.</param>
        /// <param name="newStatus">Новый статус доставки.</param>
        /// <response code="200">Статус успешно обновлён.</response>
        /// <response code="400">Некорректные параметры.</response>
        /// <response code="401">Ошибка аутентификации.</response>
        /// <response code="403">Доступ запрещён.</response>
        /// <response code="404">Заказ или пользователь не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPatch("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UpdateOrderDeliveryStatus(int orderId, int newStatus)
        {
            try
            {
                await orderService.UpdateOrderDeliveryStatusAsync(orderId, newStatus, HttpContext);
                return Ok("Статус успешно изменен.");
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(401, "Ошибка аутентификации.");
            }
            catch (AccessDeniedException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(403, ex.Message);
            }
            catch(EntityNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (InvalidParameterException ex)
            {
                logger.LogWarning(ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UserNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
