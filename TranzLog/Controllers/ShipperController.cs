using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator, Manager")]
    public class ShippersController : ControllerBase
    {
        private readonly IRepository<ShipperDTO> repo;
        private readonly ILogger<ShippersController> logger;
        public ShippersController(IRepository<ShipperDTO> shipperRepository, ILogger<ShippersController> logger) 
        {
            this.repo = shipperRepository;
            this.logger = logger;
        }
        /// <summary>
        /// Добавить нового отправителя.
        /// </summary>
        /// <param name="shipperDTO">Данные отправителя.</param>
        /// <returns>Созданный отправитель.</returns>
        /// <response code="200">Отправитель успешно добавлен.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShipperDTO>> AddShipperAsync(ShipperDTO shipperDTO)
        {
            try
            {
                var createdShipper = await repo.AddAsync(shipperDTO);
                return Ok(createdShipper);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex, "Некорректные данные.");
                return BadRequest("Некорректные данные.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить отправителя по ID.
        /// </summary>
        /// <param name="id">ID отправителя.</param>
        /// <returns>Отправитель с указанным ID.</returns>
        /// <response code="200">Отправитель найден.</response>
        /// <response code="404">Отправитель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShipperDTO>> GetShipperAsync(int id)
        {
            try
            {
                var shipper = await repo.GetAsync(id);
                if (shipper == null)
                    return NotFound($"Отправитель с ID {id} не найден.");
                return Ok(shipper);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Удалить отправителя по ID.
        /// </summary>
        /// <param name="id">ID отправителя.</param>
        /// <response code="204">Отправитель успешно удалён.</response>
        /// <response code="404">Отправитель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteShipperAsync(int id)
        {
            try
            {
                await repo.DeleteAsync(id);
                return NoContent();
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return NotFound($"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить список всех отправителей с пагинацией.
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
        /// <returns>Список отправителей.</returns>
        /// <response code="200">Список успешно получен.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<ShipperDTO>> GetAllShippers(int page = 1, int pageSize = 10)
        {
            try
            {
                var shippers = repo.GetAll();
                return Ok(shippers);
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
        /// Обновить данные отправителя.
        /// </summary>
        /// <param name="shipperDTO">Обновлённые данные отправителя.</param>
        /// <returns>Обновлённый отправитель.</returns>
        /// <response code="200">Отправитель успешно обновлён.</response>
        /// <response code="404">Отправитель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ShipperDTO>> UpdateShipperAsync(ShipperDTO shipperDTO)
        {
            try
            {
                var updateShipper = await repo.UpdateAsync(shipperDTO);
                return Ok(updateShipper);
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return NotFound($"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
