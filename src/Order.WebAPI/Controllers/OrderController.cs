using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Order.Data;
using Order.Model;
using Order.Service;
using System;
using System.Threading.Tasks;

namespace OrderService.WebAPI.Controllers
{
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrderController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> Get()
        {
            var orders = await _orderService.GetOrdersAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrderById(Guid orderId)
        {
            var order = await _orderService.GetOrderByIdAsync(orderId);
            if (order != null)
            {
                return Ok(order);
            }
            else
            {
                return NotFound();
            }
        }
      
        [HttpGet("status/GetOrderByStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrderByStatus(int status)
        {

            var orders = await _orderService.GetOrderByStatus(status);
            return Ok(orders);
        }
        [HttpGet("status/GetProfitOfAllCompletedOrderAsync")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProfitOfAllCompletedOrderAsync(int? year)
        {

            int targetYear = year ?? DateTime.Now.Year;
            var orders = await _orderService.GetProfitOfCompletedOrderAsync(year);
            return Ok(new { targetYear, orders });
        }

        [HttpGet("status/GetTotalProfitofEachMonth")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetTotalProfitofEachMonth(int? year)
        {

            int targetYear = year ?? DateTime.Now.Year;
            var orders = await _orderService.GetTotalProfitofEachMonth(year);
            return Ok(new { targetYear,orders  });
        }

        [HttpPut("orders/UpdateOrderStatus")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(Guid orderId, int status)
        {
            try
            {
                var order = await _orderService.UpdateOrderStatus(orderId, status);
                if (order == null)
                {
                    return NotFound($"Order with ID {orderId} not found.");
                }
                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("orders/CreateOrder")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateOrder([FromBody]OrderDetail order)
        {
            try
            {
                var orderResult = await _orderService.CreateOrder(order);
                if (order == null)
                {
                    return NotFound($"No input to created order.");
                }
                if(orderResult) return Ok("Order created successfully");
                return BadRequest(new { error = "Failed to create order." });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
