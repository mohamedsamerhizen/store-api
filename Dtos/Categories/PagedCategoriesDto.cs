using System.Collections.Generic;

namespace store.Dtos.Categories
{
    public class PagedCategoriesDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<CategoryListDto> Data { get; set; } = new();
    }
}