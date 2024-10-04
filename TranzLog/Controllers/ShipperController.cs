using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ShippersController : ControllerBase
    {
        private readonly IRepository<ShipperDTO> shipperRepository;
        public ShippersController(IRepository<ShipperDTO> shipperRepository) 
        {
            this.shipperRepository = shipperRepository;
        }
        [HttpPost]
        public async Task<ActionResult<ShipperDTO>> AddShipperAsync(ShipperDTO shipperDTO)
        {
            try
            {
                var createdShipper = await shipperRepository.AddAsync(shipperDTO);
                return Ok(createdShipper);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ShipperDTO>> GetShipperAsync(int id)
        {
            try
            {
                return await shipperRepository.GetAsync(id);
            }
            catch(ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteShipperAsync(int id)
        {
            try
            {
                await shipperRepository.DeleteAsync(id);
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
        [HttpGet]
        public ActionResult<IEnumerable<ShipperDTO>> GetAllShippers()
        {
            try
            {
                var shippers = shipperRepository.GetAll();
                return Ok(shippers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut]
        public async Task<ActionResult<ShipperDTO>> UpdateShipperAsync(ShipperDTO shipperDTO)
        {
            try
            {
                var updateShipper = await shipperRepository.UpdateAsync(shipperDTO);
                return Ok(updateShipper);
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
