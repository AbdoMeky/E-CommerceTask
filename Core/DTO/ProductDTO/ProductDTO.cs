using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Core.DTO.ProductDTO
{
    public class ProductDTO
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public decimal Price { get; set; }
        public IFormFile? ImagePath { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public int CategoryId { get; set; }
    }
}
