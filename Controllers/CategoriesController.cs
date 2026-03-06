using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using store.Data;
using store.Models;
using store.Dtos.Categories;
using store.Common;

namespace store.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _db;

    public CategoriesController(AppDbContext db)
    {
        _db = db;
    }

    // =========================================
    // GET CATEGORIES (Pagination + Search)
    // =========================================
    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedCategoriesDto>>> GetCategories(
        [FromQuery] CategoryQueryParams queryParams)
    {
        if (queryParams.Page <= 0) queryParams.Page = 1;
        if (queryParams.PageSize <= 0 || queryParams.PageSize > 50)
            queryParams.PageSize = 10;

        var query = _db.Categories.AsQueryable();

        if (!queryParams.IncludeInactive)
            query = query.Where(c => c.IsActive);

        if (!string.IsNullOrWhiteSpace(queryParams.Search))
        {
            var search = queryParams.Search.ToLower();
            query = query.Where(c => c.Name.ToLower().Contains(search));
        }

        var totalCount = await query.CountAsync();

        var categories = await query
            .OrderByDescending(c => c.CreatedAtUtc)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .Select(c => new CategoryListDto
            {
                Id = c.Id,
                Name = c.Name,
                Description = c.Description,
                IsActive = c.IsActive
            })
            .ToListAsync();

        var result = new PagedCategoriesDto
        {
            TotalCount = totalCount,
            Page = queryParams.Page,
            PageSize = queryParams.PageSize,
            Data = categories
        };

        return Ok(ApiResponse<PagedCategoriesDto>.SuccessResponse(result));
    }

    // =========================================
    // ADMIN: CREATE CATEGORY
    // =========================================
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CategoryCreateDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = true,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("Category created successfully"));
    }

    // =========================================
    // ADMIN: UPDATE CATEGORY
    // =========================================
    [Authorize(Roles = "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, CategoryUpdateDto dto)
    {
        var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return NotFound(ApiResponse<string>.FailResponse("Category not found"));

        category.Name = dto.Name;
        category.Description = dto.Description;
        category.IsActive = dto.IsActive;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("Category updated successfully"));
    }

    // =========================================
    // ADMIN: SOFT DELETE CATEGORY
    // =========================================
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _db.Categories
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category is null)
            return NotFound(ApiResponse<string>.FailResponse("Category not found"));

        if (category.Products.Any(p => p.IsActive))
            return BadRequest(ApiResponse<string>.FailResponse(
                "Cannot deactivate category with active products"));

        category.IsActive = false;
        category.UpdatedAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        return Ok(ApiResponse<string>.SuccessResponse("Category deactivated successfully"));
    }
}