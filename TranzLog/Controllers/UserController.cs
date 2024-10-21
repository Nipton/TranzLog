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
                logger.LogInformation(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (DuplicateException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                await repo.DeleteUserAsync(id);
                return Ok();
            }
            catch (UserNotFoundException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet]
        public ActionResult<IEnumerable<UserDTO>> GetAllUsers()
        {
            try
            {
                var users = repo.GetAllUsers();
                return Ok(users);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
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
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet("find-{userName}")]
        public async Task<ActionResult<UserDTO>> GetUserByName(string userName)
        {
            try
            {
                var user = await repo.GetUserByNameAsync(userName);
                if (user == null)
                    return StatusCode(404, $"Пользователь {userName} не найден");
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(int id)
        {
            try
            {
                var user = await repo.GetUserByIdAsync(id);
                if (user == null)
                    return StatusCode(404, $"Пользователь с ID {id} не найден");
                return Ok(user);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpPatch]
        public async Task<ActionResult> ChangeRole(string userName, string targetRole)
        {
            try
            {
                string? currentUserRole = CurrentUserProvider.GetCurrentUserInfo(HttpContext)?.Role.ToString();
                if(currentUserRole == null)
                {
                    return StatusCode(401, $"Ошибка аутентификации.");
                }
                await authenticationService.ChangeUserRole(userName, targetRole, currentUserRole);
                return Ok();
            }
            catch (UserNotFoundException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (InvalidRoleException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(400, ex.Message);
            }
            catch (UnauthorizedAccessException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(403, ex.Message);
            }
            catch (ArgumentException ex)
            {
                logger.LogInformation(ex.Message);
                return StatusCode(401, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");

            }
        }
    }
}
