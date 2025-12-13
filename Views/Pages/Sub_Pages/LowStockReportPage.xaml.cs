using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages
{
    public sealed partial class LowStockReportPage : Page
    {
        // Assuming you have a product service
        private readonly ProductServices _productService;
        private readonly PrintServiceLowStock _printService;

        // Observable collections for data binding
        public ObservableCollection<LowStockProduct> LowStockProducts { get; set; } = new ObservableCollection<LowStockProduct>();

        // Properties for summary cards
        private int _criticalCount;
        public int CriticalCount
        {
            get => _criticalCount;
            set
            {
                _criticalCount = value;
                CriticalCountText.Text = value.ToString();
            }
        }

        private int _lowStockCount;
        public int LowStockCount
        {
            get => _lowStockCount;
            set
            {
                _lowStockCount = value;
                LowStockCountText.Text = value.ToString();
            }
        }

        private int _totalItemsCount;
        public int TotalItemsCount
        {
            get => _totalItemsCount;
            set
            {
                _totalItemsCount = value;
                TotalItemsCountText.Text = value.ToString();
            }
        }

        public LowStockReportPage()
        {
            this.InitializeComponent();

            _productService = new ProductServices();
            _printService = new PrintServiceLowStock();

            this.Loaded += LowStockReportPage_Loaded;
        }

        private async void LowStockReportPage_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadLowStockData();
        }

        private async Task LoadLowStockData()
        {
            try
            {
                // Show loading indicator if you have one
                // LoadingProgressRing.IsActive = true;

                // Get all products from your service
                var allProducts = await _productService.GetProductsAsync();

                // Filter products with stock <= 50 units
                var lowStockItems = allProducts
                    .Where(p => p.Quantity <= 50)
                    .Select(p => new LowStockProduct
                    {
                        Id = p.Id,
                        ProductName = p.Name,
                        CategoryName = p.Category ?? "Uncategorized",
                        CurrentStock = p.Quantity,
                        StockLevel = p.Quantity < 10 ? StockLevel.Critical : StockLevel.Low
                    })
                    .OrderBy(p => p.CurrentStock) // Show most critical first
                    .ToList();

                // Clear and populate the collection
                LowStockProducts.Clear();
                foreach (var item in lowStockItems)
                {
                    LowStockProducts.Add(item);
                }

                // Update summary counts
                CriticalCount = lowStockItems.Count(p => p.StockLevel == StockLevel.Critical);
                LowStockCount = lowStockItems.Count(p => p.StockLevel == StockLevel.Low);
                TotalItemsCount = lowStockItems.Count;

                // Hide loading indicator
                // LoadingProgressRing.IsActive = false;
            }
            catch (Exception ex)
            {
                // Handle error
                // Show error dialog or message
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to load low stock data: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }

        private async void GenerateReportButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!LowStockProducts.Any())
                {
                    var noDataDialog = new ContentDialog
                    {
                        Title = "No Data",
                        Content = "There are no low stock items to generate a report.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await noDataDialog.ShowAsync();
                    return;
                }

                // Generate PDF report
                var pdfPath = await _printService.GenerateLowStockReportPdf(LowStockProducts.ToList());

                // Open the PDF
                await _printService.OpenPdfAsync(pdfPath);

            }
            catch (Exception ex)
            {
                var dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to generate report: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await dialog.ShowAsync();
            }
        }
        private async Task GenerateReport()
        {
            // Implement your report generation logic here
            // For example: Export to PDF, Excel, or print

            await Task.Delay(1000); // Simulate report generation

            // Example: You could use a library like ClosedXML for Excel export
            // or iTextSharp for PDF generation
        }
    }
}