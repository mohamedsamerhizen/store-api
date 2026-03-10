using Microsoft.EntityFrameworkCore;
using store.Data;
using store.Dtos.Categories;
using store.Models;

namespace store.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private readonly AppDbContext _db;

        public CategoryService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedCategoriesDto> GetCategoriesAsync(CategoryQueryParams queryParams)
        {
            if (queryParams.Page <= 0)
            {
                queryParams.Page = 1;
            }

            if (queryParams.PageSize <= 0 || queryParams.PageSize > 50)
            {
                queryParams.PageSize = 10;
            }

            var query = _db.Categories.AsQueryable();

            if (!queryParams.IncludeInactive)
            {
                query = query.Where(c => c.IsActive);
            }

            if (!string.IsNullOrWhiteSpace(queryParams.Search))
            {
                var search = queryParams.Search.Trim().ToLower();
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

            return new PagedCategoriesDto
            {
                TotalCount = totalCount,
                Page = queryParams.Page,
                PageSize = queryParams.PageSize,
                Data = categories
            };
        }

        public async Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto)
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

            return MapToResponseDto(category);
        }

        public async Task<CategoryResponseDto> UpdateAsync(int id, CategoryUpdateDto dto)
        {
            var category = await _db.Categories.FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            category.Name = dto.Name;
            category.Description = dto.Description;
            category.IsActive = dto.IsActive;
            category.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return MapToResponseDto(category);
        }

        public async Task DeactivateAsync(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category is null)
            {
                throw new KeyNotFoundException("Category not found.");
            }

            if (category.Products.Any(p => p.IsActive))
            {
                throw new InvalidOperationException("Cannot deactivate category with active products.");
            }

            category.IsActive = false;
            category.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }

        private static CategoryResponseDto MapToResponseDto(Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };
        }
    }
}