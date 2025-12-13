using System;

namespace Toyo_cable_UI.Models
{
    public class LowStockProduct
    {
        public Guid Id { get; set; }
        public string ProductName { get; set; }
        public string CategoryName { get; set; }
        public int CurrentStock { get; set; }
        public StockLevel StockLevel { get; set; }

        public string StockText => $"{CurrentStock} units";
        public string StockColor => StockLevel == StockLevel.Critical ? "#DC2626" : "#F59E0B";
        public string StockLevelText => StockLevel == StockLevel.Critical ? "Critical" : "Low";
    }

    public enum StockLevel
    {
        Critical,  
        Low        
    }
}