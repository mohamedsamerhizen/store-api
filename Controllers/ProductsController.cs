using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Common;
using store.Dtos.Products;
using store.Services.Products;

namespace store.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            int? categoryId = null)
        {
            var result = await _productService.GetProductsAsync(page, pageSize, search, categoryId);

            return Ok(ApiResponse.SuccessResponse("Products retrieved successfully.", result));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            return Ok(ApiResponse.SuccessResponse("Product retrieved successfully.", product));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto dto)
        {
            var product = await _productService.CreateProductAsync(dto);

            return StatusCode(201, ApiResponse.SuccessResponse("Product created successfully.", product));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto dto)
        {
            var product = await _productService.UpdateProductAsync(id, dto);

            return Ok(ApiResponse.SuccessResponse("Product updated successfully.", product));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            await _productService.DeleteProductAsync(id);

            return Ok(ApiResponse.SuccessResponse("Product deleted successfully."));
        }
    }
}
