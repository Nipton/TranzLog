﻿using Microsoft.AspNetCore.Authorization;
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
        [AllowAnonymous]
        [HttpPost("login")]
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
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Некорректные данные для регистрации.");
            }
            try
            {
                var reusltRegister = await authenticationService.RegisterAsync(registerDTO);
                if(reusltRegister.Success)
                {
                    return Ok(reusltRegister.Message);
                }
                else
                {
                    return Conflict(reusltRegister.Message);
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
