using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using store.Data;
using store.Dtos.Products;

namespace store.Services.Products
{
    public class ProductCacheService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        public ProductCacheService(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<PagedProductsDto> GetProductsAsync(
            int page,
            int pageSize,
            string? search,
            int? categoryId)
        {
            if (page <= 0)
            {
                page = 1;
            }

            if (pageSize <= 0 || pageSize > 50)
            {
                pageSize = 10;
            }

            var normalizedSearch = string.IsNullOrWhiteSpace(search)
                ? "all"
                : search.Trim().ToLower();

            var normalizedCategory = categoryId?.ToString() ?? "all";

            var cacheKey = $"products:page={page}:size={pageSize}:search={normalizedSearch}:category={normalizedCategory}";

            if (_cache.TryGetValue(cacheKey, out PagedProductsDto? cachedResult))
            {
                return cachedResult!;
            }

            var query = _db.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var trimmedSearch = search.Trim();
                query = query.Where(p => p.Name.Contains(trimmedSearch));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var totalCount = await query.CountAsync();

            var products = await query
                .OrderByDescending(p => p.CreatedAtUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new ProductListDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Stock = p.Stock,
                    IsActive = p.IsActive,
                    CategoryName = p.Category != null ? p.Category.Name : null
                })
                .ToListAsync();

            var result = new PagedProductsDto
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                Data = products
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));

            return result;
        }

        public void ClearCache()
        {
            if (_cache is MemoryCache memoryCache)
            {
                memoryCache.Clear();
            }
        }
    }
}