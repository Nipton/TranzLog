using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using TranzLog.Repositories;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ShippersController : ControllerBase
    {
        private readonly IRepository<ShipperDTO> repo;
        private readonly ILogger<ShippersController> logger;
        public ShippersController(IRepository<ShipperDTO> shipperRepository, ILogger<ShippersController> logger) 
        {
            this.repo = shipperRepository;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<ActionResult<ShipperDTO>> AddShipperAsync(ShipperDTO shipperDTO)
        {
            try
            {
                var createdShipper = await repo.AddAsync(shipperDTO);
                return Ok(createdShipper);
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
        public async Task<ActionResult<ShipperDTO>> GetShipperAsync(int id)
        {
            try
            {
                var shipper = await repo.GetAsync(id);
                if (shipper == null)
                    return NotFound($"Отправитель с ID {id} не найден.");
                return Ok(shipper);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteShipperAsync(int id)
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
        [HttpGet]
        public ActionResult<IEnumerable<ShipperDTO>> GetAllShippers(int page = 1, int pageSize = 10)
        {
            try
            {
                var shippers = repo.GetAll();
                return Ok(shippers);
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
        [HttpPut]
        public async Task<ActionResult<ShipperDTO>> UpdateShipperAsync(ShipperDTO shipperDTO)
        {
            try
            {
                var updateShipper = await repo.UpdateAsync(shipperDTO);
                return Ok(updateShipper);
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
