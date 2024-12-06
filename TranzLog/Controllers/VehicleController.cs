using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator, Manager")]
    public class VehicleController : ControllerBase
    {
        private readonly IRepository<VehicleDTO> repo;
        private readonly ILogger<VehicleController> logger;
        public VehicleController(IRepository<VehicleDTO> repo, ILogger<VehicleController> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }
        /// <summary>
        /// Добавить новый транспорт.
        /// </summary>
        /// <param name="vehicleDTO">Данные транспорта.</param>
        /// <returns>Созданный транспорт.</returns>
        /// <response code="200">Транспорт успешно добавлен.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="404">Связанная сущность (водитель) не найдена.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VehicleDTO>> AddVehicleAsync(VehicleDTO vehicleDTO)
        {
            try
            {
                var createdVehicle = await repo.AddAsync(vehicleDTO);
                return Ok(createdVehicle);

            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(404, ex.Message);
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
        /// Получить транспорт по ID.
        /// </summary>
        /// <param name="id">ID транспорта.</param>
        /// <returns>Транспорт с указанным ID.</returns>
        /// <response code="200">Транспорт найден.</response>
        /// <response code="404">Транспорт с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VehicleDTO>> GetVehicleAsync(int id)
        {
            try
            {
                var vehicle = await repo.GetAsync(id);
                if (vehicle == null)
                    return NotFound($"Транспорт с ID {id} не найден.");
                return Ok(vehicle);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить список всех транспортов с пагинацией.
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
        /// <returns>Список транспортов.</returns>
        /// <response code="200">Список успешно получен.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<VehicleDTO>> GetAllVehicle(int page = 1, int pageSize = 10)
        {
            try
            {
                var list = repo.GetAll();
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
        /// Удалить транспорт по ID.
        /// </summary>
        /// <param name="id">ID транспорта.</param>
        /// <response code="204">Транспорт успешно удалён.</response>
        /// <response code="404">Транспорт с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteVehicle(int id)
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
        /// Обновить данные транспорта.
        /// </summary>
        /// <param name="vehicleDTO">Обновлённые данные транспорта.</param>
        /// <returns>Обновлённый транспорт.</returns>
        /// <response code="200">Транспорт успешно обновлён.</response>
        /// <response code="404">Транспорт с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VehicleDTO>> UpdateVehicle(VehicleDTO vehicleDTO)
        {
            try
            {
                var updateVehicle = await repo.UpdateAsync(vehicleDTO);
                return Ok(updateVehicle);
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
    }
}
