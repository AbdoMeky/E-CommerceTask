using Core.DTO;
using Core.DTO.OrderDTO;

namespace Core.Interfaces
{
    public interface IOrderRepository
    {
        Task<ShowOrderDTO> GetOrder(int id, string userId);
        Task<List<ShowUserOrderDTO>> GetOrdersForUser(string userId, int page, int pageSize);
        Task<List<ShowOrderDTO>> GetOrdersForAdmin(int page, int pageSize);
        Task<IntResult> MakeItReseved(int id);
        Task<IntResult> Delete(int id, string userId);
        Task<IntResult> DeleteForAdman(int id);
    }
}
