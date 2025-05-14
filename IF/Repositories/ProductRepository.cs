using Core.DTO;
using Core.DTO.OrderItemDTO;
using Core.DTO.ProductDTO;
using Core.Entities;
using Core.Interfaces;
using Core.Utilities;
using IF.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace IF.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly string _storagePath ;
        private readonly string _backupDirPath;
        public ProductRepository(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
            _storagePath = Path.Combine(_webHostEnvironment.WebRootPath, "UploadedImagesForProductImage");
            _backupDirPath = Path.Combine(_webHostEnvironment.WebRootPath, "BackupForProductImage");
        }
        public async Task<IntResult> Add(ProductDTO product)
        {
            if (!Directory.Exists(_storagePath))
            {
                Directory.CreateDirectory(_storagePath);
            }
            if (!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId))
            {
                return new IntResult { Message = "No category has this ID." };
            }
            var filePath = AddImageHelper.CheckImagePath(product.ImagePath, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new IntResult() { Message = filePath.Message };
            }
            var newProduct = new Product
            {
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                Quantity = product.Quantity,
                ImagePath = filePath.Id
            };
            await _context.Products.AddAsync(newProduct);
            try
            {
                if (product.ImagePath is not null)
                {
                    await AddImageHelper.AddFile(product.ImagePath, newProduct.ImagePath);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(newProduct.ImagePath))
                {
                    AddImageHelper.DeleteFiles(newProduct.ImagePath);
                }
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = newProduct.Id };
        }
        public async Task<IntResult> Update(ProductDTO product, int productId)
        {
            if (!Directory.Exists(_backupDirPath))
            {
                Directory.CreateDirectory(_backupDirPath);
            }
            var newProduct =await _context.Products.FindAsync(productId);
            if (newProduct is null)
            {
                return new IntResult { Message = "No product has this ID." };
            }
            if (!await _context.Categories.AnyAsync(c => c.Id == product.CategoryId) )
            {
                return new IntResult { Message = "No category has this ID." };
            }
            var filePath = AddImageHelper.CheckImagePath(product.ImagePath, _storagePath);
            if (!string.IsNullOrEmpty(filePath.Message))
            {
                return new IntResult() { Message = filePath.Message };
            }
            newProduct.Name = product.Name;
            newProduct.Description = product.Description;
            newProduct.Price = product.Price;
            newProduct.Quantity = product.Quantity;
            newProduct.CategoryId = product.CategoryId;
            var backup = "";
            var oldPath = newProduct.ImagePath;
            try
            {
                if (product.ImagePath is not null)
                {
                    newProduct.ImagePath = filePath.Id;
                    if (!string.IsNullOrEmpty(oldPath))
                    {
                        backup = await AddImageHelper.BackupFiles(oldPath, _backupDirPath);
                    }
                    if (!string.IsNullOrEmpty(filePath.Id))
                    {
                        await AddImageHelper.AddFile(product.ImagePath, filePath.Id);
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(filePath.Id))
                {
                    AddImageHelper.DeleteFiles(filePath.Id);
                }
                if (!string.IsNullOrEmpty(backup))
                {
                    await AddImageHelper.RestoreFile(backup, oldPath);
                }
                if (!string.IsNullOrEmpty(backup))
                {
                    AddImageHelper.DeleteFiles(backup);
                }
                return new IntResult { Message = ex.Message };
            }
            if (!string.IsNullOrEmpty(backup))
            {
                AddImageHelper.DeleteFiles(backup);
            }
            return new IntResult { Id = newProduct.Id };
        }
        public async Task<IntResult> Delete(int productId)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product is null)
            {
                return new IntResult { Message = "No product has this ID." };
            }
            _context.Remove(product);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            if (!string.IsNullOrEmpty(product.ImagePath))
            {
                AddImageHelper.DeleteFiles(product.ImagePath);
            }
            return new IntResult { Id = 1 };
        }
        public async Task<IntResult> DeleteListOfProducts(List<Product> products)
        {
            if (products.IsNullOrEmpty())
            {
                return new IntResult { Id = 1 };
            }
            _context.Products.RemoveRange(products);
            return new IntResult { Id = 1 };
        }
        public async Task<List<ShowProductDTO>> GetProducts(ProductSearchDTO searchDTO)
        {
            var query = _context.Products.AsQueryable();

            if (searchDTO.CategoryID is not null && searchDTO.CategoryID != 0)
            {
                query = query.Where(x => x.CategoryId==searchDTO.CategoryID);
            }
            if (searchDTO.MinimumPrice is not null && searchDTO.MinimumPrice > 0)
            {
                query = query.Where(x => x.Price >= searchDTO.MinimumPrice);
            }
            if (searchDTO.MaximumPrice is not null && searchDTO.MaximumPrice > 0)
            {
                query = query.Where(x => x.Price <= searchDTO.MaximumPrice);
            }
            (searchDTO.PageNumber, searchDTO.PageSize) = await PaginationHelper.NormalizePaginationAsync(
                query,
                searchDTO.PageNumber,
                searchDTO.PageSize
            );

            return await query
                .Skip((searchDTO.PageNumber - 1) * searchDTO.PageSize)
                .Take(searchDTO.PageSize)
                .Select(x => new ShowProductDTO
                {
                    Id = x.Id,
                    Description = x.Description,
                    Name = x.Name,
                    Price = x.Price,
                    ImagePath = x.ImagePath,
                    CategoryName = (x.Categories == null) ? "No Category" : x.Categories.Name
                })
                .ToListAsync();
        }
        public async Task<ShowProductDTO> ShowProduct(int productId)
        {
            return await _context.Products.Where(x => x.Id == productId).Select(x => new ShowProductDTO
            {
                Id = x.Id,
                Description = x.Description,
                Name = x.Name,
                CategoryName = (x.Categories == null) ? "No Category" : x.Categories.Name,
                Price = x.Price,
                ImagePath = x.ImagePath
            }).FirstOrDefaultAsync();
        }

        public async Task<ItemPriceDTO> Decrease(int productId, int quantityNeeded)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return new ItemPriceDTO { Massage = "No Product has this Id" };
            }

            if (product.Quantity - quantityNeeded < 0)
            {
                return new ItemPriceDTO { Massage = $"There just {product.Quantity} of {product.Name}" };
            }

            product.Quantity -= quantityNeeded;

            try
            {
                await _context.SaveChangesAsync();
                return new ItemPriceDTO { price = product.Price * quantityNeeded };
            }
            catch (Exception ex)
            {
                return new ItemPriceDTO { Massage = ex.Message };
            }
        }
        public async Task<ShowProductForAdminDTO> ShowProductForAdmin(int productId)
        {
            return await _context.Products.Where(x => x.Id == productId).Select(x => new ShowProductForAdminDTO
            {
                Id = x.Id,
                Description = x.Description,
                Quantity=x.Quantity,
                Name = x.Name,
                CategoryName = (x.Categories == null) ? "No Category" : x.Categories.Name,
                Price = x.Price,
                ImagePath = x.ImagePath
            }).FirstOrDefaultAsync();
        }
        public async Task<List<ShowProductForAdminDTO>> ShowProductsForAdmin(ProductSearchDTO searchDTO)
        {
            var query = _context.Products.AsQueryable();

            if (searchDTO.CategoryID is not null && searchDTO.CategoryID != 0)
            {
                query = query.Where(x => x.CategoryId == searchDTO.CategoryID);
            }
            if (searchDTO.MinimumPrice is not null && searchDTO.MinimumPrice > 0)
            {
                query = query.Where(x => x.Price >= searchDTO.MinimumPrice);
            }
            if (searchDTO.MaximumPrice is not null && searchDTO.MaximumPrice > 0)
            {
                query = query.Where(x => x.Price <= searchDTO.MaximumPrice);
            }
            (searchDTO.PageNumber, searchDTO.PageSize) = await PaginationHelper.NormalizePaginationAsync(
                query,
                searchDTO.PageNumber,
                searchDTO.PageSize
            );

            return await query
                .Skip((searchDTO.PageNumber - 1) * searchDTO.PageSize)
                .Take(searchDTO.PageSize)
                .Select(x => new ShowProductForAdminDTO
                {
                    Id = x.Id,
                    Description = x.Description,
                    Quantity = x.Quantity,
                    Name = x.Name,
                    Price = x.Price,
                    ImagePath = x.ImagePath,
                    CategoryName = (x.Categories == null) ? "No Category" : x.Categories.Name
                })
                .ToListAsync();
        }
    }
}
