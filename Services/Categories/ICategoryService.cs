using store.Dtos.Categories;

namespace store.Services.Categories
{
    public interface ICategoryService
    {
        Task<PagedCategoriesDto> GetCategoriesAsync(CategoryQueryParams queryParams);
        Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto);
        Task<CategoryResponseDto> UpdateAsync(int id, CategoryUpdateDto dto);
        Task DeactivateAsync(int id);
    }
}