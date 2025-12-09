using System;
using System.Collections.Generic;

namespace Toyo_cable_UI.Models.DTOs.OrderDto
{
    public class CreateOrderDto
    {
        public decimal Discount { get; set; }

        public ICollection<OrderItems>? OrderItems { get; set; }
    }
}
