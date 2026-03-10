using store.Dtos.Products;

namespace store.Services.Products
{
    public interface IProductService
    {
        Task<PagedProductsDto> GetProductsAsync(int page, int pageSize, string? search, int? categoryId);
        Task<ProductResponseDto> GetProductByIdAsync(int id);
        Task<ProductResponseDto> CreateProductAsync(ProductCreateDto dto);
        Task<ProductResponseDto> UpdateProductAsync(int id, ProductUpdateDto dto);
        Task DeleteProductAsync(int id);
    }
}