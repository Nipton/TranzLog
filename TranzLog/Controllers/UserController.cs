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
    [Route("[controller]")]
    [ApiController]
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
        [HttpPut]
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
        [HttpDelete("{id}")]
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
        [HttpGet]
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
        [HttpGet("check-{userName}")]
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
        [HttpGet("find-{userName}")]
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
        [HttpGet("{id}")]
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
        [HttpPatch]
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
