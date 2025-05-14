namespace Core.DTO.ProductDTO
{
    public class ProductSearchDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public long? MinimumPrice { get; set; }
        public long? MaximumPrice { get; set; }
        public int? CategoryID { get; set; }
    }
}
