using Microsoft.UI.Xaml.Controls;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class Dashboard : Page
{
    private readonly ProductServices _productService;
    private readonly CategoryServices _categoryServices;
    private readonly OrderService _orderServices;

    public ObservableCollection<Products> Products { get; set; }
    public ObservableCollection<Products> LowStockProducts { get; set; }
    public ObservableCollection<BestSellingProduct> BestSellingProducts { get; set; }

    public Dashboard()
    {
        InitializeComponent();

        _productService = new ProductServices();
        _categoryServices = new CategoryServices();
        _orderServices = new OrderService();

        Products = new ObservableCollection<Products>();
        LowStockProducts = new ObservableCollection<Products>();
        BestSellingProducts = new ObservableCollection<BestSellingProduct>();

        LoadProducts();
        LoadCategories();
        LoadOrders(); 
    }

    // Load all products
    public async void LoadProducts()
    {
        var response = await _productService.GetProductsAsync();

        if (response != null)
        {
            Products.Clear();
            LowStockProducts.Clear();

            var productCount = response.Count;
            totalProductsText.Text = productCount.ToString();

            foreach (var product in response)
            {
                Products.Add(product);

                if (product.Quantity <= 10)
                {
                    LowStockProducts.Add(product);
                }
            }
        }
    }

    // Load all categories
    public async void LoadCategories()
    {
        var response = await _categoryServices.GetCategoriesAsync();

        if (response != null)
        {
            var categoryCount = response.Count;
            totalCategoriesText.Text = categoryCount.ToString();
        }
    }

    // Load all orders and update graph
    public async void LoadOrders()
    {
        var response = await _orderServices.GetOrdersAsync();

        if (response != null && response.Any())
        {
            // Get all products for category lookup
            var allProducts = await _productService.GetProductsAsync();

            // Best selling products logic
            var bestSelling = response
                .Where(order => order.OrderItems != null && order.OrderItems.Any())
                .SelectMany(order => order.OrderItems)
                .Where(item => !string.IsNullOrEmpty(item.ProductName))
                .GroupBy(item => item.ProductId)
                .Select(group =>
                {
                    var firstItem = group.First();
                    var productId = group.Key;

                    var category = firstItem.Product?.Category;

                    if (string.IsNullOrEmpty(category) && allProducts != null)
                    {
                        var product = allProducts.FirstOrDefault(p => p.Id == productId);
                        category = product?.Category ?? "Unknown";
                    }

                    return new BestSellingProduct
                    {
                        ProductName = firstItem.ProductName,
                        Category = category ?? "Unknown",
                        TotalQuantitySold = group.Sum(item => item.Quantity),
                        TotalRevenue = group.Sum(item => item.TotalPrice)
                    };
                })
                .OrderByDescending(product => product.TotalQuantitySold)
                .Take(10)
                .ToList();

            BestSellingProducts.Clear();
            foreach (var product in bestSelling)
            {
                BestSellingProducts.Add(product);
            }

            // Calculate monthly sales for graph
            var monthlySales = CalculateMonthlySales(response);
            UpdateSalesGraph(monthlySales);

            // Get order statistics
            var orderCount = response.Count;
            var totalAmount = response.Sum(item => item.TotalAmount);

            totalOrdersText.Text = orderCount.ToString();
            totalRevenueText.Text = $"Rs. {totalAmount:F2}";
        }
        else
        {
            // No orders, show empty graph
            UpdateSalesGraph(new double[12]);
        }
    }

    // Calculate monthly sales from orders
    private double[] CalculateMonthlySales(List<Order> orders)
    {
        // Get current year
        int currentYear = DateTime.Now.Year;

        // Initialize array for 12 months (Jan to Dec)
        double[] monthlySales = new double[12];

        // Group orders by month and sum total amounts
        var salesByMonth = orders
            .Where(o => o.OrderTime.Year == currentYear) 
            .GroupBy(o => o.OrderTime.Month) 
            .Select(g => new
            {
                Month = g.Key,
                TotalSales = (double)g.Sum(o => o.TotalAmount)
            })
            .ToList();

        // Populate the array
        foreach (var sale in salesByMonth)
        {
            monthlySales[sale.Month - 1] = sale.TotalSales; 
        }

        // Debug output
        System.Diagnostics.Debug.WriteLine("Monthly Sales:");
        for (int i = 0; i < 12; i++)
        {
            System.Diagnostics.Debug.WriteLine($"{GetMonthName(i + 1)}: Rs. {monthlySales[i]:F2}");
        }

        return monthlySales;
    }

    // Update the sales graph with data
    private void UpdateSalesGraph(double[] monthlySales)
    {
        // Clear previous plot
        MyPlotControl.Plot.Clear();

        // Create bar positions
        double[] positions = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 };

        // Add bars with custom positioning
        var bar = MyPlotControl.Plot.Add.Bars(positions, monthlySales);

        // Customize bar appearance
        bar.Color = ScottPlot.Color.FromHex("#3B82F6");

        // bars minimal gap
        foreach (var b in bar.Bars)
        {
            b.Size = 0.8;
        }

        // Add month labels at exact bar positions
        MyPlotControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            new Tick[]
            {
            new(0, "Jan"),
            new(1, "Feb"),
            new(2, "Mar"),
            new(3, "Apr"),
            new(4, "May"),
            new(5, "Jun"),
            new(6, "Jul"),
            new(7, "Aug"),
            new(8, "Sep"),
            new(9, "Oct"),
            new(10, "Nov"),
            new(11, "Dec"),
            });

        // Customize axes
        MyPlotControl.Plot.Axes.Bottom.Label.Text = "Month";
        MyPlotControl.Plot.Axes.Left.Label.Text = "Revenue (Rs.)";

        // Set X-axis limits to show all bars properly
        MyPlotControl.Plot.Axes.SetLimitsX(-0.5, 11.5); 

        // Set Y-axis to start from 0
        double maxValue = monthlySales.Max();
        if (maxValue == 0) maxValue = 1000; 
        MyPlotControl.Plot.Axes.SetLimitsY(0, maxValue * 1.1); 

        // Disable user interaction (zoom/pan)
        MyPlotControl.UserInputProcessor.IsEnabled = false;

        // Set title
        MyPlotControl.Plot.Title($"Sales Performance - {DateTime.Now.Year}");

        // Add grid for better readability
        MyPlotControl.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#E5E7EB");
        MyPlotControl.Plot.Grid.MajorLineWidth = 1;

        // Refresh the plot
        MyPlotControl.Refresh();
    }
    // Helper method to get month name
    private string GetMonthName(int month)
    {
        return month switch
        {
            1 => "January",
            2 => "February",
            3 => "March",
            4 => "April",
            5 => "May",
            6 => "June",
            7 => "July",
            8 => "August",
            9 => "September",
            10 => "October",
            11 => "November",
            12 => "December",
            _ => "Unknown"
        };
    }
}