using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet]
        public ActionResult<IEnumerable<RouteDTO>> GetAllRoute()
        {
            try
            {
                var list = repo.GetAll();
                return Ok(list);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteRoute(int id)
        {
            try
            {
                await repo.DeleteAsync(id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpPut]
        public async Task<ActionResult<RouteDTO>> UpdateCargo(RouteDTO routeDTO)
        {
            try
            {
                var updateRoute = await repo.UpdateAsync(routeDTO);
                return Ok(updateRoute);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
