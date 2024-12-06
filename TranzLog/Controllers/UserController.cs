using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System.Net.Http;
using System.Security.Claims;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;
using TranzLog.Services;

namespace TranzLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrator, Manager")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository repo;
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<UserController> logger;
        public UserController(IUserRepository repo, ILogger<UserController> logger, IAuthenticationService authenticationService)
        {
            this.repo = repo;
            this.logger = logger;
            this.authenticationService = authenticationService;
        }
        /// <summary>
        /// Обновить данные пользователя.
        /// </summary>
        /// <param name="userDTO">Обновлённые данные пользователя.</param>
        /// <returns>Обновлённый пользователь.</returns>
        /// <response code="200">Пользователь успешно обновлён.</response>
        /// <response code="404">Пользователь не найден.</response>
        /// <response code="409">Дубликат данных пользователя. Новый ID или никнейм заняты.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDTO>> UpdateUser(UserDTO userDTO)
        {
            try
            {
                var changedUser = await repo.UpdateUserAsync(userDTO);
                return Ok(changedUser);
            }
            catch (UserNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return NotFound($"{ex.Message}");
            }
            catch (DuplicateException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return Conflict($"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Удалить пользователя по ID.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        /// <response code="204">Пользователь успешно удалён.</response>
        /// <response code="404">Пользователь не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                await repo.DeleteUserAsync(id);
                return NoContent();
            }
            catch (UserNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить всех пользователей с пагинацией.
        /// </summary>
        /// <param name="page">Номер страницы (по умолчанию 1).</param>
        /// <param name="pageSize">Размер страницы (по умолчанию 10).</param>
        /// <returns>Список пользователей.</returns>
        /// <response code="200">Пользователи успешно получены.</response>
        /// <response code="400">Некорректные параметры пагинации.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<IEnumerable<UserDTO>> GetAllUsers(int page = 1, int pageSize = 10)
        {
            try
            {
                var users = repo.GetAllUsers();
                return Ok(users);
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
        /// Проверить существование пользователя.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <returns>True, если пользователь существует, иначе False.</returns>
        /// <response code="200">Проверка выполнена успешно.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("check-{userName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<bool>> ValidateUserExists(string userName)
        {
            try
            {
                bool result = await repo.UserExistsAsync(userName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить пользователя по имени.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <returns>Данные пользователя.</returns>
        /// <response code="200">Пользователь найден.</response>
        /// <response code="404">Пользователь не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("find-{userName}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDTO>> GetUserByName(string userName)
        {
            try
            {
                var user = await repo.GetUserByNameAsync(userName);
                if (user == null)
                    return NotFound($"Пользователь {userName} не найден");
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Получить пользователя по ID.
        /// </summary>
        /// <param name="id">ID пользователя.</param>
        /// <returns>Данные пользователя.</returns>
        /// <response code="200">Пользователь найден.</response>
        /// <response code="404">Пользователь не найден.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            try
            {
                var user = await repo.GetUserByIdAsync(id);
                if (user == null)
                    return NotFound($"Пользователь с ID {id} не найден");
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, ex.Message);
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Изменить роль пользователя.
        /// </summary>
        /// <param name="userName">Имя пользователя.</param>
        /// <param name="targetRole">Целевая роль.</param>
        /// <response code="200">Роль успешно изменена.</response>
        /// <response code="401">Ошибка аутентификации.</response>
        /// <response code="403">Доступ запрещён.</response>
        /// <response code="404">Пользователь не найден.</response>
        /// <response code="400">Некорректные параметры.</response>
        /// <response code="500">Внутренняя ошибка сервера.</response>
        [HttpPatch]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> ChangeRole(string userName, string targetRole)
        {
            try
            {
                string? currentUserRole = authenticationService.GetCurrentUserInfo(HttpContext)?.Role.ToString();
                if(currentUserRole == null)
                {
                    return StatusCode(401, $"Ошибка аутентификации.");
                }
                await authenticationService.ChangeUserRole(userName, targetRole, currentUserRole);
                return Ok();
            }
            catch (UserNotFoundException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (InvalidRoleException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return BadRequest(ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogWarning(ex, ex.Message);
                return StatusCode(403, ex.Message);
            }
            catch (InvalidParameterException ex)
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
    }
}
