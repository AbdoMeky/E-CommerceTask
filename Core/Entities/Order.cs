using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalPrice { get; set; }
        public bool IsRecieved { get; set; } = false;
        public List<OrderItem>? OrderItems { get; set; }
        public string? UserId { get; set; }
        public ApplicationUser? User { get; set; }
    }
}
