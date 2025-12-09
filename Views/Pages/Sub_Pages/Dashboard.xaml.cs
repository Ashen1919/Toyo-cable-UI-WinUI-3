using Microsoft.UI.Xaml.Controls;
using ScottPlot;
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
    public ObservableCollection<BestSellingProduct> BestSellingProducts { get; set; } = new ObservableCollection<BestSellingProduct>();

    public Dashboard()
    {
        InitializeComponent();

        // Axis AntiAliasing Graph
        double[] monthlySales = { 12000, 15000, 18000, 14000, 20000, 25000, 23000, 26000, 22000, 21000, 24000, 35000 };

        var bar = MyPlotControl.Plot.Add.Bars(monthlySales);

        // Add month labels
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

        //disable on-scroll zoom-in and zoom-out
        MyPlotControl.UserInputProcessor.IsEnabled = false;

        // set title
        MyPlotControl.Plot.Title("Sales Performance");

        MyPlotControl.Refresh();

        _productService = new ProductServices();
        _categoryServices = new CategoryServices();
        _orderServices = new OrderService();

        Products = new ObservableCollection<Products>();
        LowStockProducts = new ObservableCollection<Products>();

        LoadProducts();
        LoadCategories();
        LoadOrders();

    }
    
    // load all products
    public async void LoadProducts()
    {
        // get all products
        var response = await _productService.GetProductsAsync();

        if(response != null)
        {
            Products.Clear();
            LowStockProducts.Clear();

            // get product count
            var productCount = response.Count;

            // assign them to relevant card
            totalProductsText.Text = productCount.ToString();

            foreach(var product in response)
            {
                Products.Add(product);

                if(product.Quantity <= 10)
                {
                    LowStockProducts.Add(product);
                }
            }
            
        }
    }

    // load all categories
    public async void LoadCategories()
    {
        // get all categories
        var response = await _categoryServices.GetCategoriesAsync();

        // get category count
        var CategoryCount = response.Count;

        // assign them to relevant card
        totalCategoriesText.Text = CategoryCount.ToString();
    }

    // load all orders
    public async void LoadOrders()
    {
        // get all orders
        var response = await _orderServices.GetOrdersAsync();

        // best selling products logic
        var bestSelling = response
            .SelectMany(order => order.OrderItems)
            .Where(item => item.Product != null) 
            .GroupBy(item => item.ProductId)
            .Select(group => new BestSellingProduct
            {
                ProductName = group.First().Product.Name,
                Category = group.First().Product.Category,
                TotalQuantitySold = group.Sum(item => item.Quantity),
                TotalRevenue = group.Sum(item => item.TotalPrice) 
            })
            .OrderByDescending(product => product.TotalQuantitySold)
            .Take(10)
            .ToList();

        // Clear and populate the observable collection
        BestSellingProducts.Clear();
        foreach (var product in bestSelling)
        {
            BestSellingProducts.Add(product);
        }

        // get order count
        var OrderCount = response.Count;

        // get total amount
        var totalAmount = response.Sum(item => item.TotalAmount);

        // assign them to relevant card
        totalOrdersText.Text = OrderCount.ToString();
        totalRevenueText.Text = totalAmount.ToString();
    }
}
