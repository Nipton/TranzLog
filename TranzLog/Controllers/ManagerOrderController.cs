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
        /// <summary>
        /// Получить все заказы со статусом "Pending".
        /// </summary>
        /// <returns>Список заказов со статусом "Pending".</returns>
        /// <response code="200">Список заказов успешно получен.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Обновить статус заказа.
        /// </summary>
        /// <param name="orderId">ID заказа.</param>
        /// <param name="newStatus">Новый статус заказа.</param>
        /// <response code="200">Статус заказа успешно обновлён.</response>
        /// <response code="400">Некорректные параметры.</response>
        /// <response code="404">Заказ не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPatch("{orderId}/status")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
