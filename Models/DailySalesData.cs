using System;

namespace Toyo_cable_UI.Models
{
    public class DailySalesData
    {
        public string ProductName { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }
        public int OrderCount { get; set; }
    }
}