using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService authenticationService;
        public AuthController(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO userDTO)
        {
            try
            {
                string token = await authenticationService.AuthenticateAsync(userDTO);
                return Ok(token);
            }
            catch (UserNotFoundException ex)
            {
                return StatusCode(400, ex.Message);
            }
            catch (InvalidPasswordException ex)
            {
                return StatusCode(400, ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "Internal server error");
            }
        }
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO registerDTO)
        {
            try
            {
                var reusltRegister = await authenticationService.RegisterAsync(registerDTO);
                if(reusltRegister.Success)
                {
                    return Ok(reusltRegister.Message);
                }
                else
                {
                    return StatusCode(409, reusltRegister.Message);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
