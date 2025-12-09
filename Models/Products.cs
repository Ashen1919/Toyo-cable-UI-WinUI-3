using System;

namespace Toyo_cable_UI.Models
{
    public class Products
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string ImageUrl { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        // check product stock
        public string stockStatus
        {
            get {
                if (Quantity == 0)
                    return "Out of stock";
                else if (Quantity <= 5)
                    return "Low Stock";
                else
                    return "In Stock";
            }
        }

        // set background colour
        public string stockBackgroundColour
        {
            get {
                if (Quantity == 0)
                    return "#EF4444";
                else if (Quantity <= 5)
                    return "#F59E0B";
                else
                    return "#10B981";
            }
        }

    }
}
