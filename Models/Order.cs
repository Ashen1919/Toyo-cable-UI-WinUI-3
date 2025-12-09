using System;
using System.Collections.Generic;

namespace Toyo_cable_UI.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.UtcNow;

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItems>? OrderItems { get; set; }
    }
}
