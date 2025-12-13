using Microsoft.UI;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Toyo_cable_UI.Models
{
    public class Order
    {
        private bool _isReturned;
        public Guid Id { get; set; }
        public DateTime OrderTime { get; set; } = DateTime.UtcNow;

        public decimal SubTotal { get; set; }

        public decimal Discount { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<OrderItems>? OrderItems { get; set; }

    }
}