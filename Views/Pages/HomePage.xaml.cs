using Microsoft.UI.Xaml.Controls;
using System;
using Toyo_cable_UI.Views.Pages.Sub_Pages;

namespace Toyo_cable_UI.Views.Pages;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        InitializeComponent();
        contentFrame8.Navigate(typeof(Dashboard));
    }

    private void NavigationView_SelectionChanged8(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if(args.SelectedItemContainer != null)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItemContainer;
            string selectedTag = selectedItem.Tag?.ToString();

            switch (selectedTag)
            {
                case "Dashboard":
                    contentFrame8.Navigate(typeof(Dashboard));
                    break;

                case "OrderPage":
                    contentFrame8.Navigate(typeof(OrderPage));
                    break;

                case "viewOrderPage":
                    contentFrame8.Navigate(typeof(viewOrderPage));
                    break;

                case "ProductPage":
                    contentFrame8.Navigate(typeof(ProductPage));
                    break;

                case "CategoryPage":
                    contentFrame8.Navigate(typeof(CategoryPage));
                    break;

                case "DailyReportPage":
                    contentFrame8.Navigate(typeof(DailyReportPage));
                    break;

                case "MonthlyReportPage":
                    contentFrame8.Navigate(typeof(MonthlyReportPage));
                    break;

                case "LowStockReportPage":
                    contentFrame8.Navigate(typeof(LowStockReportPage));
                    break;

                default:
                    break;
            }
        }
    }

    // logout function
    private async void LogoutButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        ContentDialog dialog = new()
        {
            Title = "Logout",
            Content = "Are you sure you want to logout?",
            PrimaryButtonText = "Logout",
            CloseButtonText = "Cancel",
            XamlRoot = this.XamlRoot
        };

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Clear user session/data here
            Frame.Navigate(typeof(LoginPage));
        }
    }
}
