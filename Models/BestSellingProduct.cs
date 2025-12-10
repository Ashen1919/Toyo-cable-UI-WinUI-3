namespace Toyo_cable_UI.Models
{
    public class BestSellingProduct
    {
        public string ProductName { get; set; }
        public string Category { get; set; }
        public int TotalQuantitySold { get; set; }
        public decimal TotalRevenue { get; set; }

        public string FormattedRevenue => $"{TotalRevenue:N0}";
        public string FormattedSales => TotalQuantitySold.ToString();
    }
}
