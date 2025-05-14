using Core.DTO;
using Core.DTO.OrderDTO;
using Core.DTO.OrderItemDTO;
using Core.Interfaces;
using Core.Utilities;
using IF.Data;
using Microsoft.EntityFrameworkCore;

namespace IF.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IItemOrderRepository _itemOrderRepository;
        public OrderRepository(AppDbContext context, IItemOrderRepository itemOrderRepository)
        {
            _context = context;
            _itemOrderRepository = itemOrderRepository;
        }
        public async Task<IntResult> Delete(int id, string userId)
        {
            var order = await _context.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == id);

            if (order is null || order.UserId != userId)
            {
                return new IntResult() { Message = "No Order has this Id" };
            }
            if (order.IsRecieved)
            {
                return new IntResult() { Message = "The Order Is already Reseaved, you can not delete it." };
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var item in order.OrderItems.ToList())
                    {
                        var result = await _itemOrderRepository.DeleteOrderItem(item.Id);
                        if (result.Id == 0)
                        {
                            return new IntResult() { Message = result.Message };
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    return new IntResult { Message = ex.Message };
                }
            }
            return new IntResult() { Id = 1 };
        }
        public async Task<IntResult> DeleteForAdman(int id)
        {
            var order = await _context.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == id);
            if (order is null)
            {
                return new IntResult() { Message = "No Order has this Id" };
            }
            if (order.IsRecieved)
            {
                return new IntResult() { Message = "The Order Is already Reseaved, you can not delete it." };
            }
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    foreach (var item in order.OrderItems.ToList())
                    {
                        var result = await _itemOrderRepository.DeleteOrderItem(item.Id);
                        if (result.Id == 0)
                        {
                            return new IntResult() { Message = result.Message };
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    return new IntResult { Message = ex.Message };
                }
            }
            return new IntResult() { Id = 1 };
        }
        public async Task<ShowOrderDTO> GetOrder(int id, string userId)
        {
            var result = await _context.Orders
                .Where(x => x.Id == id)
                .Select(o => new ShowOrderDTO
                {
                    Id = o.Id,
                    IsRecieved = o.IsRecieved,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    UserName = o.User != null ? o.User.Name : "UnKnown",
                    UserId = o.UserId,
                    OrderItems = o.OrderItems.Select(x => new ShowOrderItemDTO()
                    {
                        Id = x.Id,
                        Price = x.Price,
                        ProductName = x.Product != null ? x.Product.Name : "Unknown",
                        Quantity = x.Quantity
                    }).ToList()
                }).FirstOrDefaultAsync();

            if (result is not null && result.UserId != userId)
            {
                return null;
            }
            return result;
        }

        public async Task<List<ShowOrderDTO>> GetOrdersForAdmin(int page, int pageSize)
        {
            var result = _context.Orders
                .OrderBy(x => x.IsRecieved).ThenByDescending(x => x.OrderDate).AsQueryable();
            (page, pageSize) = await PaginationHelper.NormalizePaginationAsync(result, page, pageSize);
            return await result.Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(o => new ShowOrderDTO
                {
                    Id = o.Id,
                    IsRecieved = o.IsRecieved,
                    OrderDate = o.OrderDate,
                    TotalPrice = o.TotalPrice,
                    UserName = o.User != null ? o.User.Name : "UnKnown",
                    UserId = o.UserId,
                    OrderItems = o.OrderItems.Select(x => new ShowOrderItemDTO()
                    {
                        Id = x.Id,
                        Price = x.Price,
                        ProductName = x.Product != null ? x.Product.Name : "Unknown",
                        Quantity = x.Quantity
                    }).ToList()
                })
                .ToListAsync();
        }

        public async Task<List<ShowUserOrderDTO>> GetOrdersForUser(string userId, int page, int pageSize)
        {
            var result = _context.Orders
                        .Where(x => x.UserId == userId)
                        .OrderByDescending(x => x.OrderDate).AsQueryable();
            (page, pageSize) = await PaginationHelper.NormalizePaginationAsync(result, page, pageSize);
            return await result.Select(o => new ShowUserOrderDTO
            {
                Id = o.Id,
                IsRecieved = o.IsRecieved,
                OrderDate = o.OrderDate,
                TotalPrice = o.TotalPrice,
                OrderItems = o.OrderItems.Select(x => new ShowOrderItemDTO()
                {
                    Id = x.Id,
                    Price = x.Price,
                    ProductName = x.Product != null ? x.Product.Name : "Unknown",
                    Quantity = x.Quantity
                }).ToList()
            }).ToListAsync();
        }
        public async Task<IntResult> MakeItReseved(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return new IntResult() { Message = "there is no Order has this Id" };
            }
            order.IsRecieved = true;
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult() { Id = id };
        }
    }
}
