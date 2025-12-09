using System;

namespace Toyo_cable_UI.Models.DTOs.OrderItemsDto
{
    public class CreateOrderItemsDto
    {
        public Guid ProductId { get; set; }

        public int Quantity { get; set; }

    }
}
