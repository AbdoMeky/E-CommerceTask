﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DTO.OrderItemDTO
{
    public class ShowOrderItemDTO
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string ProductName { get; set; }
    }
}
