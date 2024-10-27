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
        [HttpPost]
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
        [HttpGet("{id}")]
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
        [HttpGet]
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
        [HttpDelete("{id}")]
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
        [HttpPut]
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
