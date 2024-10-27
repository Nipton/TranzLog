using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly IRouteRepository repo;
        private readonly ILogger<RouteController> logger;
        public RouteController(IRouteRepository repo, ILogger<RouteController> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }
        [HttpPost]
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
        [HttpGet("{id}")]
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
        [HttpGet]
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
        [HttpDelete("{id}")]
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
        [HttpPut]
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
        [HttpGet("search")]
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
