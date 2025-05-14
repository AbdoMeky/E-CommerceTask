using Core.DTO;
using Core.DTO.CategoryDTO;
using Core.Entities;
using Core.Interfaces;
using IF.Data;
using Microsoft.EntityFrameworkCore;

namespace IF.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;
        public CategoryRepository(AppDbContext context,IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }
        public async Task<IntResult> Add(string categoryName)
        {
            if(await _context.Categories.AnyAsync(x => x.Name == categoryName))
            {
                return new IntResult { Message = "there is category with this name already." };
            }
            var newCategory = new Category { Name = categoryName };
            _context.Categories.Add(newCategory);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = newCategory.Id };
        }

        public async Task<IntResult> Delete(int categoryId)
        {
            var category = await _context.Categories.FindAsync(categoryId);
            if (category is null)
            {
                return new IntResult { Message = "No category has this ID." };
            }
            _context.Categories.Remove(category);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = 1 };
        }

        public async Task<IntResult> DeleteWithDeleteProductsOnIt(int categoryId)
        {
            var category =await _context.Categories.Include(x => x.Products).FirstOrDefaultAsync(x => x.Id == categoryId);
            if (category is null)
            {
                return new IntResult { Message = "No category has this ID." };
            }
            _context.Categories.Remove(category);
            await _productRepository.DeleteListOfProducts(category.Products);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = 1 };
        }

        public async Task<List<ShowCategoryDTO>> GetAll()
        {
            return await _context.Categories.Select(x => new ShowCategoryDTO
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
        }

        public async Task<ShowCategoryDTO> GetById(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
                return null;
            return new ShowCategoryDTO
            {
                Id = category.Id,
                Name = category.Name
            };
        }
    }
}
