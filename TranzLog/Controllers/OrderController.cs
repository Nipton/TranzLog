using Microsoft.AspNetCore.Mvc;
using TranzLog.Interfaces;
using TranzLog.Models.DTO;

namespace TranzLog.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrderController : ControllerBase
    {
        private readonly ITransportOrderRepository repo;
        public OrderController(ITransportOrderRepository repo)
        {
            this.repo = repo;
        }
        [HttpPost]
        public async Task<ActionResult<TransportOrderDTO>> AddOrderAsync(TransportOrderDTO orderDTO)
        {
            try
            {
                var createdOrder = await repo.AddAsync(orderDTO);
                return Ok(createdOrder);

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet("{id}")]
        public async Task<ActionResult<TransportOrderDTO>> GetOrderAsync(int id)
        {
            try
            {
                var order = await repo.GetAsync(id);
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpGet]
        public ActionResult<IEnumerable<TransportOrderDTO>> GetAllOrder()
        {
            try
            {
                var list = repo.GetAll();
                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteOrder(int id)
        {
            try
            {
                await repo.DeleteAsync(id);
                return Ok();
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        [HttpPut]
        public async Task<ActionResult<TransportOrderDTO>> UpdateOrder(TransportOrderDTO orderDTO)
        {
            try
            {
                var updateOrder = await repo.UpdateAsync(orderDTO);
                return Ok(updateOrder);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(404, $"{ex.Message}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
