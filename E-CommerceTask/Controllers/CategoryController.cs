using Core.DTO;
using Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace E_CommerceTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository;
        public CategoryController(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }
        [HttpPost]
        [Authorize(Roles ="Admin")]
        public async Task<ActionResult> Add(string categoryName)
        {
            var result=await _categoryRepository.Add(categoryName);
            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = await _categoryRepository.GetById(result.Id) });
        }
        [HttpGet]
        public async Task<ActionResult> GetAll()
        {
            var result=await _categoryRepository.GetAll();
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int id)
        {
            var result=await _categoryRepository.Delete(id);
            if(result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess=false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true });
        }
        [HttpDelete("DeleteCategoryWithProducts")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteCategoryWithProducts(int id)
        {
            var result = await _categoryRepository.DeleteWithDeleteProductsOnIt(id);
            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true });
        }
    }
}
