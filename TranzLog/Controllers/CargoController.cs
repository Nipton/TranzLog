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
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        [HttpGet]
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
        [HttpDelete("{id}")]
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
