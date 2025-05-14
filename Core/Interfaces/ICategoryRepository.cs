using Core.DTO;
using Core.DTO.CategoryDTO;

namespace Core.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IntResult> Add(string categoryName);
        Task<IntResult> Delete(int categoryId);
        Task<List<ShowCategoryDTO>> GetAll();
        Task<ShowCategoryDTO> GetById(int id);
        Task<IntResult> DeleteWithDeleteProductsOnIt(int categoryId);
    }
}
