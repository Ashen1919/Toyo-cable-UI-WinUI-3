using System;

namespace Toyo_cable_UI.Models
{
    public class OrderItems
    {
        public Guid Id { get; set; }

        public Guid OrderId { get; set; }

        public Order? Order { get; set; }

        public Guid ProductId { get; set; }

        public Products? Product { get; set; }
        public string ProductName { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }

        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
