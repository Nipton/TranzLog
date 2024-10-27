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
    public class VehicleController : ControllerBase
    {
        private readonly IRepository<VehicleDTO> repo;
        private readonly ILogger<VehicleController> logger;
        public VehicleController(IRepository<VehicleDTO> repo, ILogger<VehicleController> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }
        [HttpPost]
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
        [HttpGet("{id}")]
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
        [HttpGet]
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
        [HttpDelete("{id}")]
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
        [HttpPut]
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
