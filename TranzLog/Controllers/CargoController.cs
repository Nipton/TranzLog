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
    [Route("[controller]")]
    public class CargoController : ControllerBase
    {
        private readonly IRepository<CargoDTO> repo;
        private readonly ILogger<CargoController> logger;
        public CargoController(IRepository<CargoDTO> repo, ILogger<CargoController> logger)
        {
            this.repo = repo;
            this.logger = logger;
        }
        [HttpPost]
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
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet("{id}")]
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet]
        [Authorize(Roles = "Administrator, Manager")]
        public ActionResult<IEnumerable<CargoDTO>> GetAllCargo()
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
        public async Task<ActionResult> DeleteCargo(int id)
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
                return StatusCode(404, ex.Message);
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
    }
}
