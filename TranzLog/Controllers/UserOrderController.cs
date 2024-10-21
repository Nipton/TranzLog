using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserOrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        private readonly ILogger<UserOrderController> logger;
        public UserOrderController(IOrderService orderService, ILogger<UserOrderController> logger)
        {
            this.orderService = orderService;
            this.logger = logger;
        }
        [HttpPost]
        public async Task<ActionResult<int>> CreateOrder(UserOrderDTO userOrderDTO)
        {
            try
            {
                var ordrderId = await orderService.CreateOrder(userOrderDTO, HttpContext);
                return Ok(ordrderId);
            }
            catch(ArgumentException ex) 
            {
                logger.LogError(ex.Message);
                return BadRequest("Неполные данные для создания заказа.");
            }
            catch(UnauthorizedAccessException ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(401, "Ошибка аутентификации.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
