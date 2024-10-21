using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public CargoController(IRepository<CargoDTO> repo)
        {
            this.repo = repo;
        }
        [HttpPost]
        public async Task<ActionResult<CargoDTO>> AddCargoAsync(CargoDTO cargoDTO)
        {
            try
            {
                var createdCargo = await repo.AddAsync(cargoDTO);
                return Ok(createdCargo);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<CargoDTO>> GetCargoAsync(int id)
        {
            try
            {
                var cargo = await repo.GetAsync(id);
                return Ok(cargo);
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
