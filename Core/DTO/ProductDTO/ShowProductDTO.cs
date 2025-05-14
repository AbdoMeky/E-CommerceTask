namespace Core.DTO.ProductDTO
{
    public class ShowProductDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }
        public string? CategoryName { get; set; }
    }
}
