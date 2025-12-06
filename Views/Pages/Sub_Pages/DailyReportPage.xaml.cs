using Microsoft.UI.Xaml.Controls;
using ScottPlot;
using System;


namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class DailyReportPage : Page
{
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
    }
}
