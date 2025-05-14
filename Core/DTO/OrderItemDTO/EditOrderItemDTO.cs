using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.OrderItemDTO
{
    public class EditOrderItemDTO
    {
        [Required]
        public int Id { get; set; }
        [Required]
        [Range(0, int.MaxValue)]
        public int Quantity { get; set; }
    }
}
