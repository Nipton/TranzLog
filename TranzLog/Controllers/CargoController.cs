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
    public class CargoController : ControllerBase
    {
        private readonly IRepository<CargoDTO> repo;
        private readonly ILogger<CargoController> logger;
        public CargoController(IRepository<CargoDTO> repo, ILogger<CargoController> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }
        /// <summary>
        /// Добавить новый груз.
        /// </summary>
        /// <param name="cargoDTO">Данные нового груза.</param>
        /// <returns>Созданный груз.</returns>
        /// <response code="200">Груз успешно добавлен.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="404">Связанная сущность(заказ) не найдена.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CargoDTO>> AddCargoAsync(CargoDTO cargoDTO)
        {
            try
            {
                var createdCargo = await repo.AddAsync(cargoDTO);
                return Ok(createdCargo);

            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return NotFound(ex.Message);
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
        /// Получить груз по ID.
        /// </summary>
        /// <param name="id">ID груза.</param>
        /// <returns>Груз с указанным ID.</returns>
        /// <response code="200">Груз найден.</response>
        /// <response code="404">Груз с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CargoDTO>> GetCargoAsync(int id)
        {
            try
            {
                var cargo = await repo.GetAsync(id);
                if (cargo == null)
                    return NotFound($"Груз с ID {id} не найден.");
                return Ok(cargo);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить список всех грузов с пагинацией.
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
        /// <returns>Список грузов.</returns>
        /// <response code="200">Грузы успешно получены.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<CargoDTO>> GetAllCargo(int page = 1, int pageSize = 10)
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
        /// Удалить груз по ID.
        /// </summary>
        /// <param name="id">ID груза.</param>
        /// <response code="204">Груз успешно удалён.</response>
        /// <response code="404">Груз с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteCargo(int id)
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
        /// Обновить данные груза.
        /// </summary>
        /// <param name="cargoDTO">Груз с обновленными данными</param>
        /// <returns>Обновлённый груз.</returns>
        /// <response code="200">Груз успешно обновлён.</response>
        /// <response code="404">Груз с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<CargoDTO>> UpdateCargo(CargoDTO cargoDTO)
        {
            try
            {
                var updateCargo = await repo.UpdateAsync(cargoDTO);
                return Ok(updateCargo);
            }
            catch (EntityNotFoundException ex)
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
