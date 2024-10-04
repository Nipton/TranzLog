using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class VehicleController : ControllerBase
    {
        private readonly IRepository<VehicleDTO> repo;
        public VehicleController(IRepository<VehicleDTO> repo)
        {
            this.repo = repo;
        }
        [HttpPost]
        public async Task<ActionResult<VehicleDTO>> AddVehicleAsync(VehicleDTO vehicleDTO)
        {
            try
            {
                var createdVehicle = await repo.AddAsync(vehicleDTO);
                return Ok(createdVehicle);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<VehicleDTO>> GetVehicleAsync(int id)
        {
            try
            {
                var vehicle = await repo.GetAsync(id);
                return Ok(vehicle);
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
        public ActionResult<IEnumerable<VehicleDTO>> GetAllVehicle()
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
        public async Task<ActionResult> DeleteVehicle(int id)
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
        public async Task<ActionResult<VehicleDTO>> UpdateVehicle(VehicleDTO vehicleDTO)
        {
            try
            {
                var updateVehicle = await repo.UpdateAsync(vehicleDTO);
                return Ok(updateVehicle);
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
