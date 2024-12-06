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
        /// <summary>
        /// Добавить нового получателя.
        /// </summary>
        /// <param name="consignee">Данные нового получателя.</param>
        /// <returns>Созданный получатель.</returns>
        /// <response code="200">Получатель успешно добавлен.</response>
        /// <response code="400">Некорректные данные.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Получить данные о получателе по ID.
        /// </summary>
        /// <param name="id">ID получателя.</param>
        /// <returns>Данные получателя.</returns>
        /// <response code="200">Получатель найден.</response>
        /// <response code="404">Получатель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Получить список всех получателей.
        /// </summary>
        /// <returns>Список получателей.</returns>
        /// <response code="200">Список получателей успешно получен.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Удалить получателя по ID.
        /// </summary>
        /// <param name="id">ID получателя.</param>
        /// <response code="200">Получатель успешно удалён.</response>
        /// <response code="404">Получатель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
        /// <summary>
        /// Обновить данные получателя.
        /// </summary>
        /// <param name="consignee">Обновлённые данные получателя.</param>
        /// <returns>Обновлённый получатель.</returns>
        /// <response code="200">Получатель успешно обновлён.</response>
        /// <response code="404">Получатель с указанным ID не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
