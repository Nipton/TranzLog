using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DriverController : ControllerBase
    {
        IDriverRepository repo;
        public DriverController(IDriverRepository repository) 
        { 
            repo = repository;
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<DriverDTO>> GetDriverAsync(int id)
        {
            try
            {
                var driver = await repo.GetAsync(id);
                return Ok(driver);
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
        public ActionResult<IEnumerable<DriverDTO>> GetAllDrivers()
        {
            try
            {
                var drivers = repo.GetAll();
                return Ok(drivers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
