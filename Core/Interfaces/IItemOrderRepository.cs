using Core.DTO;
using Core.DTO.OrderItemDTO;

namespace Core.Interfaces
{
    public interface IItemOrderRepository
    {
        Task<ShowOrderItemDTO> GetById(int id, string userId);
        Task<IntResult> AddOrderItemInOrderDidnotReseived(AddOrderItemDTO item, string userId);
        Task<IntResult> UpdateOrderItem(EditOrderItemDTO item, string userId);
        Task<IntResult> DeleteOrderItem(int id);
        Task<IntResult> DeleteOrderItemWithUserId(int id, string userId);
    }
}
