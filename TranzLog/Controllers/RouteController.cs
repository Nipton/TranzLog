using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class RouteController : ControllerBase
    {
        private readonly IRouteRepository repo;
        public RouteController(IRouteRepository repo)
        {
            this.repo = repo;
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<RouteDTO>> GetRouteAsync(int id)
        {
            try
            {
                var route = await repo.GetAsync(id);
                return Ok(route);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
            catch (Exception)
            {
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
