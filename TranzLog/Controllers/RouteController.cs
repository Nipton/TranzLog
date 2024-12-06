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
    public class RouteController : ControllerBase
    {
        private readonly IRouteRepository repo;
        private readonly ILogger<RouteController> logger;
        public RouteController(IRouteRepository repo, ILogger<RouteController> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }
        /// <summary>
        /// Добавить новый маршрут.
        /// </summary>
        /// <param name="routeDTO">Данные маршрута.</param>
        /// <returns>Созданный маршрут.</returns>
        /// <response code="200">Маршрут успешно добавлен.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RouteDTO>> AddRouteAsync(RouteDTO routeDTO)
        {
            try
            {               
                var createdRoute = await repo.AddAsync(routeDTO);
                return Ok(createdRoute);

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
        /// Получить маршрут по ID.
        /// </summary>
        /// <param name="id">ID маршрута.</param>
        /// <returns>Маршрут с указанным ID.</returns>
        /// <response code="200">Маршрут найден.</response>
        /// <response code="404">Маршрут с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RouteDTO>> GetRouteAsync(int id)
        {
            try
            {
                var route = await repo.GetAsync(id);
                if (route == null)
                    return NotFound($"Маршрут с ID {id} не найден.");
                return Ok(route);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить список всех маршрутов.
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
        /// <returns>Список маршрутов.</returns>
        /// <response code="200">Список маршрутов успешно получен.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<RouteDTO>> GetAllRoute(int page = 1, int pageSize = 10)
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
        /// Удалить маршрут по ID.
        /// </summary>
        /// <param name="id">ID маршрута.</param>
        /// <response code="204">Маршрут успешно удалён.</response>
        /// <response code="404">Маршрут с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteRoute(int id)
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
        /// Обновить данные маршрута.
        /// </summary>
        /// <param name="routeDTO">Обновлённые данные маршрута.</param>
        /// <returns>Обновлённый маршрут.</returns>
        /// <response code="200">Маршрут успешно обновлён.</response>
        /// <response code="404">Маршрут с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RouteDTO>> UpdateRoute(RouteDTO routeDTO)
        {
            try
            {
                var updateRoute = await repo.UpdateAsync(routeDTO);
                return Ok(updateRoute);
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
        /// Поиск маршрутов по точкам отправления и назначения.
        /// </summary>
        /// <param name="from">Пункт отправления.</param>
        /// <param name="to">Пункт назначения.</param>
        /// <returns>Маршрут, соответствующий критериям.</returns>
        /// <response code="200">Маршрут найден.</response>
        /// <response code="404">Маршрут не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("search")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<RouteDTO>> GetRoutesAsync(string from, string to)
        {
            try
            {
                var route = await repo.GetRoutesAsync(from, to);
                if (route != null)
                    return Ok(route);
                else return NotFound("Маршрут не найден.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
