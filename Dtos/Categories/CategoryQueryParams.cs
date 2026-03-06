namespace store.Dtos.Categories
{
    public class CategoryQueryParams
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Search { get; set; }
        public bool IncludeInactive { get; set; } = false;
    }
}