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
        private readonly ILogger<ShippersController> logger;
        public ShippersController(IRepository<ShipperDTO> shipperRepository, ILogger<ShippersController> logger) 
        {
            this.shipperRepository = shipperRepository;
            this.logger = logger;
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ShipperDTO>> GetShipperAsync(int id)
        {
            try
            {
                var shipper = await shipperRepository.GetAsync(id);
                if (shipper == null)
                    return NotFound($"Отправитель с ID {id} не найден.");
                return Ok(shipper);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
