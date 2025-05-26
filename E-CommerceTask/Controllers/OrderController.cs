using Core.DTO;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_CommerceTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _orderRepository;
        public OrderController(IOrderRepository orderRepository)
        {
            _orderRepository = orderRepository;
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> GetById(int id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _orderRepository.GetOrder(id, userId);
            if (result is null)
            {
                return NotFound(new Generalresponse { IsSuccess = false });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpGet("OrdersForUser")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> GetOrdersForUser(int page, int pageSize)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _orderRepository.GetOrdersForUser(userId, page, pageSize);
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpGet("CurrentUserOrder")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> GetCurrentUserOrder()
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _orderRepository.GetCurrentUserOrder(userId);
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpGet("OrdersForAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetOrdersForAdmin(int page, int pageSize)
        {
            var result = await _orderRepository.GetOrdersForAdmin(page, pageSize);
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpPut("MarkAsReceived/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> MarkAsReceived(int id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _orderRepository.MakeItReseved(id);

            if (result.Id == 0)
            {
                return NotFound(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true });
        }
        [HttpDelete("Delete/{id:int}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> DeleteForUser(int id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _orderRepository.Delete(id, userId);
            if (result.Id == 0)
            {
                return NotFound(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true });
        }
        [HttpDelete("DeleteForAdmin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteForAdmin(int id)
        {
            var result = await _orderRepository.DeleteForAdman(id);
            if (result.Id == 0)
            {
                return NotFound(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true });
        }
    }
}
