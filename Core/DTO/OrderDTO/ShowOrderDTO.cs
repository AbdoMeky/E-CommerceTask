using Core.DTO.OrderItemDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.OrderDTO
{
    public class ShowOrderDTO
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? UserName { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalPrice { get; set; }
        public bool IsRecieved { get; set; }
        public List<ShowOrderItemDTO>? OrderItems { get; set; }
    }
}
