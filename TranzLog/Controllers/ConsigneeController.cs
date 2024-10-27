using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrator, Manager")]
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
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
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
        public async Task<ActionResult> DeleteConsignee(int id)
        {
            try
            {
                await repo.DeleteAsync(id);
                return Ok();
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
        public async Task<ActionResult<ConsigneeDTO>> UpdateConsigneeAsync(ConsigneeDTO consignee)
        {
            try
            {
                var updatedConsignee = await repo.UpdateAsync(consignee);
                return Ok(updatedConsignee);
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
