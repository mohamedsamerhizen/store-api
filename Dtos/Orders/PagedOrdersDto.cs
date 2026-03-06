using System.Collections.Generic;

namespace store.Dtos.Orders
{
    public class PagedOrdersDto
    {
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public List<OrderDto> Data { get; set; } = new();
    }
}