using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConsigneeController : ControllerBase
    {
        private readonly IRepository<ConsigneeDTO> repo;
        private readonly ILogger<ConsigneeController> logger;
        public ConsigneeController(IRepository<ConsigneeDTO> repository, ILogger<ConsigneeController> logger)
        {
            repo = repository;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<ActionResult<ConsigneeDTO>> AddConsigneeAsync(ConsigneeDTO consignee)
        {
            try
            {
                var createdConsignee = await repo.AddAsync(consignee);
                return Ok(createdConsignee);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsigneeDTO>> GetConsigneeAsync(int id)
        {
            try
            {
                var consignee = await repo.GetAsync(id);
                if (consignee == null) 
                    return NotFound($"Получатель с ID {id} не найден.");
                return Ok(consignee);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet]
        public ActionResult<IEnumerable<ConsigneeDTO>> GetAllConsignee()
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
        public async Task<ActionResult> DeleteConsignee(int id)
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
        public async Task<ActionResult<ConsigneeDTO>> UpdateConsigneeAsync(ConsigneeDTO consignee)
        {
            try
            {
                return await repo.UpdateAsync(consignee);
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
