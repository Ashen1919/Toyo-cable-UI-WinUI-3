using Microsoft.UI.Xaml.Controls;
using ScottPlot;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class Dashboard : Page
{
    private readonly AuthService _authService;

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

        // auth service
        _authService = new AuthService();

    }



}
