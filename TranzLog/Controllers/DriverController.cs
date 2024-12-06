using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator, Manager")]
    public class DriverController : ControllerBase
    {
        private readonly IDriverRepository repo;
        private readonly ILogger<DriverController> logger;
        public DriverController(IDriverRepository repository, ILogger<DriverController> logger) 
        { 
            repo = repository;
            this.logger = logger;
        }
        /// <summary>
        /// Добавить нового водителя.
        /// </summary>
        /// <param name="driverDTO">Данные нового водителя.</param>
        /// <returns>Созданный водитель.</returns>
        /// <response code="200">Водитель успешно добавлен.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DriverDTO>> AddDriverAsync(DriverDTO driverDTO)
        {
            try
            {
                var createdDriver = await repo.AddAsync(driverDTO);
                return Ok(createdDriver);
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
        /// Получить данные водителя по ID.
        /// </summary>
        /// <param name="id">ID водителя.</param>
        /// <returns>Данные водителя.</returns>
        /// <response code="200">Водитель найден.</response>
        /// <response code="404">Водитель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DriverDTO>> GetDriverAsync(int id)
        {
            try
            {
                var driver = await repo.GetAsync(id);
                if (driver == null)
                    return NotFound($"Водитель с ID {id} не найден.");
                return Ok(driver);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить список всех водителей.
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
        /// <returns>Список водителей.</returns>
        /// <response code="200">Список водителей успешно получен.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<DriverDTO>> GetAllDrivers(int page = 1, int pageSize = 10)
        {
            try
            {
                var drivers = repo.GetAll();
                return Ok(drivers);
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
        /// Удалить водителя по ID.
        /// </summary>
        /// <param name="id">ID водителя.</param>
        /// <response code="204">Водитель успешно удалён.</response>
        /// <response code="404">Водитель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteDriverAsync(int id)
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
        /// Обновить данные водителя.
        /// </summary>
        /// <param name="driver">Обновлённые данные водителя.</param>
        /// <returns>Обновлённый водитель.</returns>
        /// <response code="200">Данные водителя успешно обновлены.</response>
        /// <response code="404">Водитель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<DriverDTO>> UpdateDriverAsync(DriverDTO driver)
        {
            try
            {
                var updateDriver = await repo.UpdateAsync(driver);
                return Ok(updateDriver);
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
