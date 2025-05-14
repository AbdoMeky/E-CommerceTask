using Core.DTO;
using Core.DTO.OrderItemDTO;
using Core.Interfaces;
using E_CommerceTask.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace E_CommerceTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemOrderController : ControllerBase
    {
        private readonly IItemOrderRepository _itemOrderRepository;
        public ItemOrderController(IItemOrderRepository itemOrderRepository)
        {
            _itemOrderRepository = itemOrderRepository;
        }
        [HttpGet("{id:int}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> Get(int id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _itemOrderRepository.GetById(id, userId);
            if (result is null)
            {
                return NotFound(new Generalresponse { IsSuccess = false });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> Add(AddOrderItemDTO item)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Generalresponse { IsSuccess = false, Data = ModelState.ExtractErrors() });
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _itemOrderRepository.AddOrderItemInOrderDidnotReseived(item, userId);

            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true ,Data=await _itemOrderRepository.GetById(result.Id,userId)});
        }
        [HttpPut]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> Update(EditOrderItemDTO item)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Generalresponse { IsSuccess = false, Data = ModelState.ExtractErrors() });
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _itemOrderRepository.UpdateOrderItem(item, userId);

            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = await _itemOrderRepository.GetById(result.Id, userId) });
        }
        [HttpDelete("User/{id:int}")]
        [Authorize(Roles = "User")]
        public async Task<ActionResult> DeleteForUser(int id)
        {
            var userId = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new Generalresponse { IsSuccess = false, Data = "User not logged in." });
            var result = await _itemOrderRepository.DeleteOrderItemWithUserId(id, userId);

            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true });
        }
        [HttpDelete("Admin/{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteForAdmin(int id)
        {
            var result = await _itemOrderRepository.DeleteOrderItem(id);

            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true });
        }
    }
}
