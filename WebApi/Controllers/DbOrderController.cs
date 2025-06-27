using Entities;
using Data;
using Microsoft.AspNetCore.Mvc;
using Core.Services.DbOrders;
using Microsoft.AspNetCore.Authorization;
using WebApi.Models;
using Humanizer;

namespace WebApi.Controllers
{

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DbOrderController : ControllerBase
    {
        private readonly ISqlOrderService _orderService;

        public DbOrderController(ISqlOrderService OrderService)
        { 
            _orderService = OrderService;
        }

        [Route("create")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] DbOrderModel model)
        {
            var order = new DbOrder
            {
                ProductName = model.ProductName,
                Quantity = model.Quantity,
                UnitPrice = model.UnitPrice
            };

            await _orderService.AddAsync(order);
            return CreatedAtAction(nameof(GetById), new { id = order.id }, order);
        }

        [Route("{id}/GetById")]
        [HttpGet]
        public async Task<IActionResult> GetById(string id)
        {
            var order = await _orderService.GetOrderAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [Route("List")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            return Ok( await _orderService.GetAllAsync());
        }

        [Route("{id}/DeleteById")]
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var order = await _orderService.GetOrderAsync(id);
            if (order == null) 
                return NotFound();

            await _orderService.DeleteAsync(order);

            return NoContent();
        }
    }
}
