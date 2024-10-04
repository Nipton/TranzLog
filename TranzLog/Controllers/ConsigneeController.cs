using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ConsigneeController : ControllerBase
    {
        IRepository<ConsigneeDTO> repo;
        public ConsigneeController(IRepository<ConsigneeDTO> repository)
        {
            repo = repository;
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<ConsigneeDTO>> GetConsigneeAsync(int id)
        {
            try
            {
                return await repo.GetAsync(id);
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
        public ActionResult<IEnumerable<ConsigneeDTO>> GetAllConsignee()
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
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
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
