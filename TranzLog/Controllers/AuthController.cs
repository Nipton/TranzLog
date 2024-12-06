using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService authenticationService;
        private readonly ILogger<AuthController> logger;
        public AuthController(IAuthenticationService authenticationService, ILogger<AuthController> logger)
        {
            this.authenticationService = authenticationService;
            this.logger = logger;
        }
        /// <summary>
        /// Аутентификация пользователя.
        /// </summary>
        /// <param name="userDTO">Данные для входа (логин и пароль).</param>
        /// <returns>Токен доступа в случае успешной аутентификации.</returns>
        /// <response code="200">Успешная аутентификация. Возвращает токен доступа.</response>
        /// <response code="400">Некорректные данные для входа.</response>
        /// <response code="401">Пользователь не найден или указан неверный пароль.</response>
        /// <response code="500">Ошибка сервера во время аутентификации.</response>
        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<string>> Login(LoginDTO userDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для входа.");
            }
            try
            {
                string token = await authenticationService.AuthenticateAsync(userDTO);
                return Ok(token);
            }
            catch (UserNotFoundException ex)
            {
                return StatusCode(401, ex.Message);
            }
            catch (InvalidPasswordException ex)
            {
                return StatusCode(401, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка во время аутентификации пользователя {userDTO.UserName}: {ex}");
                return StatusCode(500, "Ошибка сервера");
            }
        }
        /// <summary>
        /// Регистрация нового пользователя.
        /// </summary>
        /// <param name="registerDTO">Данные для регистрации (имя пользователя, пароль и т.д.).</param>
        /// <returns>Сообщение о результате регистрации.</returns>
        /// <response code="200">Успешная регистрация. Возвращает сообщение об успехе.</response>
        /// <response code="400">Некорректные данные для регистрации.</response>
        /// <response code="409">Пользователь с указанными данными уже существует.</response>
        /// <response code="500">Ошибка сервера во время регистрации.</response>
        [AllowAnonymous]
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для регистрации.");
            }
            try
            {
                var resultRegister = await authenticationService.RegisterAsync(registerDTO);
                if(resultRegister.Success)
                {
                    return Ok(resultRegister.Message);
                }
                else
                {
                    return Conflict(resultRegister.Message);
                }
            }
            catch (Exception ex)
            {
                logger.LogError($"Ошибка во время регистрации пользователя {registerDTO.UserName}: {ex}");
                return StatusCode(500, "Ошибка сервера");
            }
        }
    }
}
