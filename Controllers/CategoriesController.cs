using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Common;
using store.Dtos.Categories;
using store.Services.Categories;

namespace store.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories([FromQuery] CategoryQueryParams queryParams)
        {
            var result = await _categoryService.GetCategoriesAsync(queryParams);

            return Ok(ApiResponse.SuccessResponse("Categories retrieved successfully.", result));
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CategoryCreateDto dto)
        {
            var category = await _categoryService.CreateAsync(dto);

            return StatusCode(201, ApiResponse.SuccessResponse("Category created successfully.", category));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryUpdateDto dto)
        {
            var category = await _categoryService.UpdateAsync(id, dto);

            return Ok(ApiResponse.SuccessResponse("Category updated successfully.", category));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            await _categoryService.DeactivateAsync(id);

            return Ok(ApiResponse.SuccessResponse("Category deactivated successfully."));
        }
    }
}
