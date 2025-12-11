using Microsoft.UI.Xaml.Controls;
using ScottPlot;
using System;
using System.Diagnostics;
using System.Linq;
using Toyo_cable_UI.Services;


namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class DailyReportPage : Page
{
    private readonly OrderService _orderService;

    public DailyReportPage()
    {
        InitializeComponent();
        TodayDateText.Text = "Today: " + DateTime.Now.ToString("dd/MM/yyyy");

        // Axis AntiAliasing Graph
        double[] monthlySales = { 12000, 15000, 18000, 14000, 20000, 25000 };

        var bar = MyPlotControl.Plot.Add.Bars(monthlySales);

        // Add month labels
        MyPlotControl.Plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(
            new Tick[]
            {
        new(0, "Acc. Cable"),
        new(1, "Three-Wheel Cable"),
        new(2, "Bike Cable"),
        new(3, "Spark Plug"),
        new(4, "Lights"),
        new(5, "Side Mirror"),
            });

        //disable on-scroll zoom-in and zoom-out
        MyPlotControl.UserInputProcessor.IsEnabled = false;

        MyPlotControl.Refresh();

        // order service
        _orderService = new OrderService();

        LoadData();
    }

    // load all card data
    public async void LoadData()
    {
        var orders = await _orderService.GetOrdersAsync();

        var today = DateTime.Now.Date;

        var filteredOrder = orders
            .Where(o => o.OrderTime.Date == today).ToList();

        if( filteredOrder.Count > 0)
        {
            DailyTotalOrdersText.Text = filteredOrder.Count.ToString();

            DailyTotalRevenueText.Text = $"Rs. {filteredOrder.Sum(o => o.TotalAmount):N2}";

            DailyAvgRevenueText.Text = $"Rs. {((filteredOrder.Sum(o => o.TotalAmount))/(filteredOrder.Count)):N2}";

            var orderItems = filteredOrder.SelectMany(o => o.OrderItems).ToList();
            if( orderItems.Count > 0 )
            {
                DailyTotalItemSoldText.Text = orderItems.Sum(oi => oi.Quantity).ToString();
            }
            else
            {
                Debug.WriteLine("order count is null");
            }
        }
        else
        {
            Debug.WriteLine("Filter Products are null");
        }
        
    }
}
