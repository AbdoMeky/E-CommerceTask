using Core.DTO;
using Core.DTO.OrderItemDTO;
using Core.Entities;
using Core.Interfaces;
using IF.Data;
using Microsoft.EntityFrameworkCore;

namespace IF.Repositories
{
    public class ItemOrderRepository : IItemOrderRepository
    {
        private readonly AppDbContext _context;
        private readonly IProductRepository _productRepository;
        public ItemOrderRepository(AppDbContext context,IProductRepository productRepository)
        {
            _context = context;
            _productRepository = productRepository;
        }
        public async Task<IntResult> AddOrderItemInOrderDidnotReseived(AddOrderItemDTO item, string userId)
        {
            var user = await _context.Users.Include(x => x.Orders).FirstOrDefaultAsync(x => x.Id == userId);
            if (user is null)
                return new IntResult() { Message = "No User has this Id" };

            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is null)
                return new IntResult() { Message = "No Product has this Id" };

            var order = user.Orders.FirstOrDefault(x => !x.IsRecieved);
            if (order is null)
            {
                order = new Order { UserId = userId };
                await _context.Orders.AddAsync(order);
            }
            var resultId = 0;
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _context.SaveChangesAsync();
                var result = await AddOrderItemInSpacificOrder(new AddOrderItemDTO
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }, order.Id);

                if (result.Id == 0)
                    return new IntResult() { Message = result.Message };

                resultId = result.Id;
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                return new IntResult() { Message = ex.Message };
            }
            return new IntResult() { Id = resultId };
        }

        private async Task<IntResult> AddOrderItemInSpacificOrder(AddOrderItemDTO item, int orderId)
        {
            var order = await _context.Orders.Include(x => x.OrderItems).FirstOrDefaultAsync(x => x.Id == orderId);
            if (order is null)
                return new IntResult() { Message = "No Order has this Id" };

            if (order.IsRecieved)
                return new IntResult() { Message = "The Order has reseived, we could not change it" };

            var product = await _context.Products.FindAsync(item.ProductId);
            if (product is null)
                return new IntResult { Message = $"No product has this Id: {item.ProductId}" };

            try
            {
                var result = await _productRepository.Decrease(item.ProductId, item.Quantity);
                if (!string.IsNullOrEmpty(result.Massage))
                    return new IntResult() { Message = result.Massage };

                var newItem = new OrderItem
                {
                    OrderId = orderId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    Price = (decimal)result.price
                };

                await _context.OrderItems.AddAsync(newItem);
                order.TotalPrice += newItem.Price;
                await _context.SaveChangesAsync();

                return new IntResult { Id = newItem.Id };
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
        }
        public async Task<IntResult> DeleteOrderItem(int id)
        {
            var item = await GetWithOrder(id);
            if (item == null)
                return new IntResult() { Message = "No Order Item has this Id" };

            return await DeleteItem(item);
        }

        public async Task<IntResult> DeleteOrderItemWithUserId(int id, string userId)
        {
            var item = await GetWithOrder(id);
            if (item == null || item.Order.UserId != userId)
                return new IntResult() { Message = "No Order Item has this Id" };

            return await DeleteItem(item);
        }

        public async Task<ShowOrderItemDTO> GetById(int id, string userId)
        {
            var item = await _context.OrderItems
                            .Include(x => x.Product)
                            .Include(x => x.Order)
                            .FirstOrDefaultAsync(x => x.Id == id);

            if (item == null || item.Order?.UserId != userId)
                return null;
            return new ShowOrderItemDTO()
            {
                Id = item.Id,
                Price = item.Price,
                ProductName = item.Product is not null ? item.Product.Name : "Deleted Product",
                Quantity = item.Quantity
            };
        }

        public async Task<IntResult> UpdateOrderItem(EditOrderItemDTO item, string userId)
        {
            var oldItem = await GetWithOrder(item.Id);
            if (oldItem == null || oldItem.Order.UserId != userId)
                return new IntResult { Message = "No Order Item has this Id" };

            if (oldItem.Order.IsRecieved)
                return new IntResult { Message = "The Order is already received" };

            if (item.Quantity == 0)
                return await DeleteItem(oldItem);

            var changedQuantity = item.Quantity - oldItem.Quantity;

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var result = await _productRepository.Decrease((int)oldItem.ProductId, changedQuantity);
                if (!string.IsNullOrEmpty(result.Massage))
                    return new IntResult() { Message = result.Massage };

                oldItem.Quantity += changedQuantity;
                oldItem.Price += (decimal)result.price;
                oldItem.Order.TotalPrice += (decimal)result.price;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                return new IntResult { Message = ex.Message };
            }
            return new IntResult { Id = oldItem.Id };
        }
        private async Task<IntResult> DeleteItem(OrderItem item)
        {
            if (item.Order.IsRecieved)
                return new IntResult() { Message = "The Order is already received. We cannot delete this item." };

            _context.OrderItems.Remove(item);
            try
            {
                var result = await _productRepository.Decrease((int)item.ProductId, -1 * item.Quantity);
                if (!string.IsNullOrEmpty(result.Massage) && result.Massage != "No Product has this Id")
                    return new IntResult() { Message = result.Massage };
                await _context.SaveChangesAsync();
                var order = item.Order;
                if (order is not null)
                {
                    order.TotalPrice -= item.Price;
                    if (!order.OrderItems.Any())
                        _context.Orders.Remove(order);
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return new IntResult() { Message = ex.Message };
            }
            return new IntResult() { Id = 1 };
        }
        private async Task<OrderItem> GetWithOrder(int id)
        {
            return await _context.OrderItems
                .Include(x => x.Order).ThenInclude(x => x.OrderItems)
                .FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}
