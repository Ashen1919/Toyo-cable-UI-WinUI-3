using System;

namespace Toyo_cable_UI.Models.DTOs.ProductDto
{
    public class UpdateRequestDto
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
    }
}
