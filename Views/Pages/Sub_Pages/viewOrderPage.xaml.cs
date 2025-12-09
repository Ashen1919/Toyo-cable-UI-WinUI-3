using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;


namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class viewOrderPage : Page
{
    private readonly OrderService _orderService;

    public ObservableCollection<Order> Orders { get; set; }

    public viewOrderPage()
    {
        InitializeComponent();

        _orderService = new OrderService();
        Orders = new ObservableCollection<Order>();

        LoadOrders();
        
    }

    // load orders
    public async void LoadOrders()
    {
        var orders = await _orderService.GetOrdersAsync();

        if (orders != null)
        {
            Orders.Clear();
            foreach (var order in orders)
            {
                Orders.Add(order);
            }
        }
    }
}
