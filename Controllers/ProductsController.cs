using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using store.Common;
using store.Data;
using store.Models;
using store.Services.Products;

namespace store.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ProductCacheService _cacheService;

        public ProductsController(
            AppDbContext db,
            ProductCacheService cacheService)
        {
            _db = db;
            _cacheService = cacheService;
        }

        //////////////////////////////////////////////////////////
        // Get Products
        //////////////////////////////////////////////////////////

        [HttpGet]
        [EnableRateLimiting("api")]
        [ResponseCache(Duration = 60)]
        public async Task<IActionResult> GetProducts(
            int page = 1,
            int pageSize = 10,
            string? search = null,
            int? categoryId = null)
        {
            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            var total = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Products retrieved successfully",
                Data = new
                {
                    TotalCount = total,
                    Page = page,
                    PageSize = pageSize,
                    Items = products
                }
            });
        }

        //////////////////////////////////////////////////////////
        // Get Product By Id
        //////////////////////////////////////////////////////////

        [HttpGet("{id}")]
        [EnableRateLimiting("api")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Product retrieved successfully",
                Data = product
            });
        }

        //////////////////////////////////////////////////////////
        // Create Product (Admin)
        //////////////////////////////////////////////////////////

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            product.CreatedAtUtc = DateTime.UtcNow;

            _db.Products.Add(product);

            await _db.SaveChangesAsync();

            _cacheService.ClearCache();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Product created successfully",
                Data = product
            });
        }

        //////////////////////////////////////////////////////////
        // Update Product (Admin)
        //////////////////////////////////////////////////////////

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateProduct(
            int id,
            Product updated)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            product.Name = updated.Name;
            product.Description = updated.Description;
            product.Price = updated.Price;
            product.Stock = updated.Stock;
            product.CategoryId = updated.CategoryId;
            product.ImageUrl = updated.ImageUrl;
            product.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _cacheService.ClearCache();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Product updated successfully",
                Data = product
            });
        }

        //////////////////////////////////////////////////////////
        // Soft Delete Product (Admin)
        //////////////////////////////////////////////////////////

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound(new ApiResponse
                {
                    Success = false,
                    Message = "Product not found"
                });
            }

            product.IsActive = false;
            product.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _cacheService.ClearCache();

            return Ok(new ApiResponse
            {
                Success = true,
                Message = "Product deleted successfully"
            });
        }
    }
}