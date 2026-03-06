using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using store.Data;
using store.Models;

namespace store.Services.Products
{
    public class ProductCacheService
    {
        private readonly AppDbContext _db;
        private readonly IMemoryCache _cache;

        private const string CacheKey = "products_cache";

        public ProductCacheService(AppDbContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<List<Product>> GetProductsAsync()
        {
            if (_cache.TryGetValue(CacheKey, out List<Product>? products))
            {
                return products!;
            }

            products = await _db.Products
                .Where(p => p.IsActive)
                .ToListAsync();

            _cache.Set(
                CacheKey,
                products,
                TimeSpan.FromMinutes(5));

            return products;
        }

        public void ClearCache()
        {
            _cache.Remove(CacheKey);
        }
    }
}