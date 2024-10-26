using Microsoft.AspNetCore.Mvc;
using TranzLog.Exceptions;
using TranzLog.Interfaces;
using TranzLog.Models;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService orderService;
        private readonly ILogger<OrderController> logger;
        public OrderController(ITransportOrderRepository repo, ILogger<OrderController> logger, IOrderService orderService)
        {
            this.logger = logger;
            this.orderService = orderService;
        }
        [HttpPost]
        public async Task<ActionResult<TransportOrderDTO>> AddOrderAsync(TransportOrderDTO orderDTO)
        {
            try
            {                
                var createdOrder = await orderService.CreateOrderAsync(orderDTO);
                return Ok(createdOrder);

            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TransportOrderDTO>> GetOrderAsync(int id)
        {
            try
            {
                var order = await orderService.GetOrderAsync(id);
                if (order == null)
                    return NotFound($"Заказ с ID {id} не найден.");
                return Ok(order);
            }
            catch (InvalidParameterException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpGet]
        public ActionResult<IEnumerable<TransportOrderDTO>> GetAllOrder(int page = 1, int pageSize = 10)
        {
            try
            {
                var list = orderService.GetAll(page, pageSize);
                return Ok(list);
            }
            catch (ArgumentException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(404, $"Неверный запрос.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            try
            {
                await orderService.DeleteAsync(id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
        [HttpPut]
        public async Task<ActionResult<TransportOrderDTO>> UpdateOrder(TransportOrderDTO orderDTO)
        {
            try
            {
                var updateOrder = await orderService.UpdateOrderAsync(orderDTO);
                return Ok(updateOrder);
            }
            catch (EntityNotFoundException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(404, ex.Message);
            }
            catch (InvalidParameterException ex)
            {
                logger.LogWarning(ex.Message);
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
                return StatusCode(500, $"Internal server error");
            }
        }
    }
}
