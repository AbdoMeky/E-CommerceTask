using Core.DTO;
using Core.DTO.ProductDTO;
using Core.Interfaces;
using E_CommerceTask.Extentions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace E_CommerceTask.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        public ProductController(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Add(ProductDTO product)
        {
            if (!ModelState.IsValid)
                return BadRequest(BadRequest(new Generalresponse { IsSuccess = false, Data = ModelState.ExtractErrors() }));
            var result=await _productRepository.Add(product);
            if(result.Id==0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = await _productRepository.ShowProduct(result.Id) });
        }
        [HttpPut("{productId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Update(ProductDTO product,int productId)
        {
            if (!ModelState.IsValid)
                return BadRequest(BadRequest(new Generalresponse { IsSuccess = false, Data = ModelState.ExtractErrors() }));
            var result = await _productRepository.Update(product, productId);
            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = await _productRepository.ShowProduct(result.Id) });
        }
        [HttpDelete("{productId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> Delete(int productId)
        {
            var result= await _productRepository.Delete(productId);
            if (result.Id == 0)
            {
                return BadRequest(new Generalresponse { IsSuccess = false, Data = result.Message });
            }
            return Ok(new Generalresponse { IsSuccess = true }); ;
        }
        [HttpGet("User/{productId:int}")]
        public async Task<ActionResult>GetbyId(int productId)
        {
            var result = await _productRepository.ShowProduct(productId);
            if(result is null)
            {
                return NotFound(new Generalresponse { IsSuccess = false });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpGet("GetAllProducts")]
        public async Task<ActionResult> GetProductsInCategory([FromQuery]ProductSearchDTO searchDTO)
        {
            var result = await _productRepository.GetProducts(searchDTO);
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpGet("Admin/{productId:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetbyIdForAmin(int productId)
        {
            var result = await _productRepository.ShowProductForAdmin(productId);
            if (result is null)
            {
                return NotFound(new Generalresponse { IsSuccess = false });
            }
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
        [HttpGet("GetAllProductsForAdmin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetProductsInCategoryForAdmin([FromQuery] ProductSearchDTO searchDTO)
        {
            var result = await _productRepository.ShowProductsForAdmin(searchDTO);
            return Ok(new Generalresponse { IsSuccess = true, Data = result });
        }
    }
}
