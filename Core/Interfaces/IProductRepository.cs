using Core.DTO;
using Core.DTO.OrderItemDTO;
using Core.DTO.ProductDTO;
using Core.Entities;

namespace Core.Interfaces
{
    public interface IProductRepository
    {
        Task<IntResult> Add(ProductDTO product);
        Task<IntResult> Update(ProductDTO product, int productId);
        Task<IntResult> Delete(int productId);
        Task<List<ShowProductDTO>> GetProducts(ProductSearchDTO searchDTO);
        Task<ShowProductDTO> ShowProduct(int productId);
        Task<IntResult> DeleteListOfProducts(List<Product> products);
        Task<ItemPriceDTO> Decrease(int productId, int quantityNeeded);
        Task<ShowProductForAdminDTO> ShowProductForAdmin(int productId);
        Task<List<ShowProductForAdminDTO>> ShowProductsForAdmin(ProductSearchDTO searchDTO);
    }
}
