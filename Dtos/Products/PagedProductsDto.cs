using System.Collections.Generic;

namespace store.Dtos.Products
{
    public class PagedProductsDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }

        public List<ProductListDto> Data { get; set; } = new();
    }
}