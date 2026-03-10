using Microsoft.EntityFrameworkCore;
using store.Data;
using store.Dtos.Products;
using store.Models;

namespace store.Services.Products
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _db;
        private readonly ProductCacheService _cacheService;

        public ProductService(AppDbContext db, ProductCacheService cacheService)
        {
            _db = db;
            _cacheService = cacheService;
        }

        public async Task<PagedProductsDto> GetProductsAsync(int page, int pageSize, string? search, int? categoryId)
        {
            return await _cacheService.GetProductsAsync(page, pageSize, search, categoryId);
        }

        public async Task<ProductResponseDto> GetProductByIdAsync(int id)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (product is null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            return MapToProductResponseDto(product);
        }

        public async Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto)
        {
            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Selected category is invalid or inactive.");
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Stock = dto.Stock,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                IsActive = true,
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();

            product = await _db.Products
                .Include(p => p.Category)
                .FirstAsync(p => p.Id == product.Id);

            _cacheService.ClearCache();

            return MapToProductResponseDto(product);
        }

        public async Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto dto)
        {
            var product = await _db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            var categoryExists = await _db.Categories
                .AnyAsync(c => c.Id == dto.CategoryId && c.IsActive);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Selected category is invalid or inactive.");
            }

            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.CategoryId = dto.CategoryId;
            product.ImageUrl = dto.ImageUrl;
            product.IsActive = dto.IsActive;
            product.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            product = await _db.Products
                .Include(p => p.Category)
                .FirstAsync(p => p.Id == product.Id);

            _cacheService.ClearCache();

            return MapToProductResponseDto(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _db.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product is null)
            {
                throw new KeyNotFoundException("Product not found.");
            }

            product.IsActive = false;
            product.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _cacheService.ClearCache();
        }

        private static ProductResponseDto MapToProductResponseDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Stock = product.Stock,
                IsActive = product.IsActive
            };
        }
    }
}