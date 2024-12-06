using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator, Manager")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        private readonly ILogger<OrderController> logger;
        public OrderController(ITransportOrderRepository repo, ILogger<OrderController> logger, IOrderService orderService)
        {
            this.logger = logger;
            this.orderService = orderService;
        }
        /// <summary>
        /// Добавить новый заказ.
        /// </summary>
        /// <param name="orderDTO">Данные нового заказа.</param>
        /// <returns>Созданный заказ.</returns>
        /// <response code="200">Заказ успешно добавлен.</response>
        /// <response code="404">Связанные сущности не найдены. (Отправитель, грузоперевозчик, маршрут, транспорт)</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransportOrderDTO>> AddOrderAsync(TransportOrderDTO orderDTO)
        {
            try
            {                
                var createdOrder = await orderService.CreateOrderAsync(orderDTO);
                return Ok(createdOrder);

            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить заказ по ID.
        /// </summary>
        /// <param name="id">ID заказа.</param>
        /// <returns>Заказ с указанным ID.</returns>
        /// <response code="200">Заказ найден.</response>
        /// <response code="400">Некорректные данные ID.</response>
        /// <response code="404">Заказ с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransportOrderDTO>> GetOrderAsync(int id)
        {
            try
            {
                var order = await orderService.GetOrderAsync(id);
                if (order == null)
                    return NotFound($"Заказ с ID {id} не найден.");
                return Ok(order);
            }
            catch (InvalidParameterException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить список всех заказов с пагинацией.
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
        /// <returns>Список заказов.</returns>
        /// <response code="200">Заказы успешно получены.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<TransportOrderDTO>> GetAllOrder(int page = 1, int pageSize = 10)
        {
            try
            {
                var list = orderService.GetAll(page, pageSize);
                return Ok(list);
            }
            catch (InvalidPaginationParameterException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Удалить заказ по ID.
        /// </summary>
        /// <param name="id">ID заказа.</param>
        /// <response code="204">Заказ успешно удалён.</response>
        /// <response code="404">Заказ с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            try
            {
                await orderService.DeleteAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Обновить данные заказа.
        /// </summary>
        /// <param name="orderDTO">Обновлённые данные заказа.</param>
        /// <returns>Обновлённый заказ.</returns>
        /// <response code="200">Заказ успешно обновлён.</response>
        /// <response code="404">Заказ с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransportOrderDTO>> UpdateOrder(TransportOrderDTO orderDTO)
        {
            try
            {
                var updateOrder = await orderService.UpdateOrderAsync(orderDTO);
                return Ok(updateOrder);
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
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
