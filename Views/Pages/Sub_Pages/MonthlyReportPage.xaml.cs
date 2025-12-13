using Microsoft.UI.Xaml.Controls;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class MonthlyReportPage : Page
{
    private readonly OrderService _orderService;
    private readonly PrintServiceSales _printServiceSales;

    public ObservableCollection<DailySalesData> MonthlySalesDataList { get; set; }

    private DateTime _selectedMonth = DateTime.Now.Date;

    // Store current statistics for report generation
    private int _currentTotalOrders = 0;
    private decimal _currentTotalRevenue = 0;
    private decimal _currentAvgRevenue = 0;
    private int _currentTotalItemsSold = 0;

    public MonthlyReportPage()
    {
        InitializeComponent();

        MonthText.Text = "This Month: " + DateTime.Now.ToString("MMMM yyyy");

        // Initialize services
        _orderService = new OrderService();
        _printServiceSales = new PrintServiceSales();

        // Initialize collections
        MonthlySalesDataList = new ObservableCollection<DailySalesData>();

        // Load data for current month
        LoadData(_selectedMonth);
    }

    private void SetEmptyState()
    {
        _currentTotalOrders = 0;
        _currentTotalRevenue = 0;
        _currentAvgRevenue = 0;
        _currentTotalItemsSold = 0;

        DailyTotalOrdersText.Text = "0";
        DailyTotalRevenueText.Text = "Rs. 0.00";
        DailyAvgRevenueText.Text = "Rs. 0.00";
        DailyTotalItemSoldText.Text = "0";

        MonthlySalesDataList.Clear();
        InitializeEmptyGraph();
    }

    // Event: Load button clicked
    private void LoadMonthButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Load data for selected month
        LoadData(_selectedMonth);
    }

    // Load data for specific month
    public async void LoadData(DateTime selectedMonth)
    {
        var orders = await _orderService.GetOrdersAsync();

        if (orders == null || !orders.Any())
        {
            Debug.WriteLine("No orders found");
            SetEmptyState();
            return;
        }

        // Filter orders for selected month and year
        var filteredOrders = orders
            .Where(o => o.OrderTime.Year == selectedMonth.Year &&
                       o.OrderTime.Month == selectedMonth.Month)
            .ToList();

        if (filteredOrders.Count > 0)
        {
            _currentTotalOrders = filteredOrders.Count;
            _currentTotalRevenue = filteredOrders.Sum(o => o.TotalAmount);
            _currentAvgRevenue = _currentTotalRevenue / _currentTotalOrders;

            var orderItems = filteredOrders
                .SelectMany(o => o.OrderItems)
                .Where(oi => oi != null)
                .ToList();

            _currentTotalItemsSold = orderItems.Sum(oi => oi.Quantity);

            // Update cards
            DailyTotalOrdersText.Text = _currentTotalOrders.ToString();
            DailyTotalRevenueText.Text = $"Rs. {_currentTotalRevenue:N2}";
            DailyAvgRevenueText.Text = $"Rs. {_currentAvgRevenue:N2}";
            DailyTotalItemSoldText.Text = _currentTotalItemsSold.ToString();

            // Calculate product sales data
            var monthlySalesData = CalculateMonthlySales(filteredOrders);

            // Populate table
            MonthlySalesDataList.Clear();
            foreach (var item in monthlySalesData)
            {
                MonthlySalesDataList.Add(item);
            }

            // Update graph
            UpdateSalesGraph(monthlySalesData, selectedMonth);

            Debug.WriteLine($"Data loaded for {selectedMonth:MMMM yyyy}");
            Debug.WriteLine($"Stored stats - Orders: {_currentTotalOrders}, Revenue: {_currentTotalRevenue}");
        }
        else
        {
            Debug.WriteLine($"No orders for {selectedMonth:MMMM yyyy}");
            SetEmptyState();
        }
    }

    private void MonthListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        var selectedItem = e.ClickedItem as ListViewItem;
        if (selectedItem != null)
        {
            string monthText = selectedItem.Content.ToString();
            DateTime selectedMonth = ParseMonthFromString(monthText);
            _selectedMonth = selectedMonth;
            LoadData(selectedMonth);
        }
    }

    private void LoadMonthsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var selectedItems = MonthListView.SelectedItems;

        if (selectedItems.Count == 0)
        {
            // No selection, show message
            ContentDialog noSelectionDialog = new ContentDialog
            {
                Title = "No Month Selected",
                Content = "Please select at least one month to load data.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            _ = noSelectionDialog.ShowAsync();
            return;
        }

        if (selectedItems.Count == 1)
        {
            // Single month selected
            var selectedItem = selectedItems[0] as ListViewItem;
            string monthText = selectedItem.Content.ToString();
            DateTime selectedMonth = ParseMonthFromString(monthText);
            _selectedMonth = selectedMonth;
            LoadData(selectedMonth);
        }
        else
        {
            // Multiple months selected - load first one for now
            // You can enhance this to show combined data
            var firstItem = selectedItems[0] as ListViewItem;
            string monthText = firstItem.Content.ToString();
            DateTime selectedMonth = ParseMonthFromString(monthText);
            _selectedMonth = selectedMonth;
            LoadData(selectedMonth);
        }
    }

    private DateTime ParseMonthFromString(string monthString)
    {
        try
        {
            // Parse "January 2025" format
            return DateTime.ParseExact(monthString, "MMMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
        }
        catch
        {
            // Return current month if parsing fails
            return DateTime.Now;
        }
    }

    // Calculate monthly sales by product
    private List<DailySalesData> CalculateMonthlySales(List<Order> orders)
    {
        var salesData = orders
            .SelectMany(order => order.OrderItems)
            .Where(item => item != null && !string.IsNullOrEmpty(item.ProductName))
            .GroupBy(item => item.ProductName)
            .Select(group => new DailySalesData
            {
                ProductName = group.Key,
                TotalQuantitySold = group.Sum(item => item.Quantity),
                TotalRevenue = group.Sum(item => item.TotalPrice),
                OrderCount = group.Select(item => item.OrderId).Distinct().Count()
            })
            .OrderByDescending(data => data.TotalRevenue)
            .ToList();

        return salesData;
    }

    // Update the bar graph with month parameter
    private void UpdateSalesGraph(List<DailySalesData> salesData, DateTime month)
    {
        MyPlotControl.Plot.Clear();

        if (salesData == null || !salesData.Any())
        {
            InitializeEmptyGraph();
            return;
        }

        double[] revenues = salesData.Select(s => (double)s.TotalRevenue).ToArray();
        double[] positions = Enumerable.Range(0, salesData.Count).Select(i => (double)i).ToArray();
        string[] productNames = salesData.Select(s => TruncateProductName(s.ProductName, 15)).ToArray();

        var barPlot = MyPlotControl.Plot.Add.Bars(positions, revenues);

        for (int i = 0; i < barPlot.Bars.Count; i++)
        {
            barPlot.Bars[i].Size = 0.8;
            barPlot.Bars[i].FillColor = ScottPlot.Color.FromHex("#3B82F6");
            barPlot.Bars[i].LineWidth = 0;
        }

        var ticks = positions.Select((pos, i) => new Tick(pos, productNames[i])).ToArray();
        MyPlotControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(ticks);

        MyPlotControl.Plot.Axes.Bottom.TickLabelStyle.Rotation = -45;
        MyPlotControl.Plot.Axes.Bottom.TickLabelStyle.Alignment = ScottPlot.Alignment.MiddleRight;

        MyPlotControl.Plot.Axes.Bottom.Label.Text = "Products";
        MyPlotControl.Plot.Axes.Bottom.Label.FontSize = 14;
        MyPlotControl.Plot.Axes.Left.Label.Text = "Revenue (Rs.)";
        MyPlotControl.Plot.Axes.Left.Label.FontSize = 14;

        MyPlotControl.Plot.Axes.SetLimitsX(-0.5, positions.Length - 0.5);

        double maxRevenue = revenues.Max();
        if (maxRevenue == 0) maxRevenue = 1000;
        MyPlotControl.Plot.Axes.SetLimitsY(0, maxRevenue * 1.15);

        MyPlotControl.Plot.Grid.MajorLineColor = ScottPlot.Color.FromHex("#E5E7EB");
        MyPlotControl.Plot.Grid.MajorLineWidth = 1;

        // Update title with selected month
        MyPlotControl.Plot.Title($"Top Selling Products - {month:MMMM yyyy}");

        MyPlotControl.UserInputProcessor.IsEnabled = false;
        MyPlotControl.Refresh();
    }

    private void InitializeEmptyGraph()
    {
        MyPlotControl.Plot.Clear();

        var text = MyPlotControl.Plot.Add.Text($"No sales data for {_selectedMonth:MMMM yyyy}", 0.5, 0.5);
        text.LabelFontSize = 16;
        text.LabelFontColor = ScottPlot.Color.FromHex("#9CA3AF");
        text.LabelAlignment = ScottPlot.Alignment.MiddleCenter;

        MyPlotControl.Plot.Axes.SetLimitsX(0, 1);
        MyPlotControl.Plot.Axes.SetLimitsY(0, 1);

        MyPlotControl.Plot.HideGrid();
        MyPlotControl.Plot.Layout.Frameless();

        MyPlotControl.UserInputProcessor.IsEnabled = false;
        MyPlotControl.Refresh();
    }

    private string TruncateProductName(string name, int maxLength)
    {
        if (string.IsNullOrEmpty(name)) return "Unknown";

        return name.Length <= maxLength
            ? name
            : name.Substring(0, maxLength - 3) + "...";
    }

    private async void PrintReport_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (MonthlySalesDataList.Count == 0)
        {
            ContentDialog noDataDialog = new ContentDialog
            {
                Title = "No Data",
                Content = "No sales data available for the selected month.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await noDataDialog.ShowAsync();
            return;
        }

        try
        {
            var salesDataList = MonthlySalesDataList.ToList();

            var pdfPath = await _printServiceSales.GenerateDailySalesReportPdf
             (
                reportDate: _selectedMonth,
                salesData: salesDataList,
                totalOrders: _currentTotalOrders,
                totalRevenue: _currentTotalRevenue,
                avgRevenue: _currentAvgRevenue,
                totalItemsSold: _currentTotalItemsSold
            );

            // Auto-open for printing
            await _printServiceSales.OpenPdfAsync(pdfPath);
        }
        catch (Exception ex)
        {
            ContentDialog errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to print report: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }
}