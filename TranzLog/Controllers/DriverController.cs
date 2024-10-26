using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
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
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet]
        public ActionResult<IEnumerable<DriverDTO>> GetAllDrivers()
        {
            try
            {
                var drivers = repo.GetAll();
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteDriverAsync(int id)
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
        public async Task<ActionResult<DriverDTO>> UpdateDriverAsync(DriverDTO driver)
        {
            try
            {
                var updateDriver = await repo.UpdateAsync(driver);
                return Ok(updateDriver);
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
