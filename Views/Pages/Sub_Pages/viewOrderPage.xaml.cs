using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class viewOrderPage : Page
{
    private readonly OrderService _orderService;
    private readonly PrintService _printService; 
    public ObservableCollection<Order> Orders { get; set; }
    public ObservableCollection<CartItem> CartItems { get; set; }

    // All orders cache
    private List<Order> allOrders;

    // Filter properties
    private DateTime? startDate = null;
    private DateTime? endDate = null;
    private bool isStartDateSelected = false;
    private bool isEndDateSelected = false;

    // Pagination properties
    private int currentPage = 1;
    private int pageSize = 10;
    private int totalPages = 1;
    private decimal _subTotal;
    private decimal _discount;
    private decimal _totalPayment;

    // Current order details for printing
    private Order _currentOrderDetails;

    public viewOrderPage()
    {
        InitializeComponent();
        _orderService = new OrderService();
        _printService = new PrintService(); 
        Orders = new ObservableCollection<Order>();
        allOrders = new List<Order>();

        // Subscribe to date picker events
        StartDatePicker.DateChanged += (s, e) => isStartDateSelected = true;
        EndDatePicker.DateChanged += (s, e) => isEndDateSelected = true;

        LoadOrders();
    }

    // Load all orders initially
    public async void LoadOrders()
    {
        var orders = await _orderService.GetOrdersAsync();

        if (orders != null)
        {
            // Store all orders and sort by date descending (newest first)
            allOrders = orders.OrderByDescending(o => o.OrderTime).ToList();

            // Reset to page 1
            currentPage = 1;

            // Apply pagination
            ApplyFiltersAndPagination();
        }
    }

    // Filter button click handler
    private void FilterButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        // Get selected dates from DatePickers only if they were actually selected
        startDate = isStartDateSelected ? StartDatePicker.Date.DateTime : null;
        endDate = isEndDateSelected ? EndDatePicker.Date.DateTime : null;

        // Reset to page 1 when filtering
        currentPage = 1;

        // Apply filters and pagination
        ApplyFiltersAndPagination();
    }

    // Apply filters and pagination
    private void ApplyFiltersAndPagination()
    {
        // Start with all orders
        var filteredOrders = allOrders.AsEnumerable();

        // Apply date filters if set
        if (startDate.HasValue)
        {
            filteredOrders = filteredOrders.Where(o => o.OrderTime >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Add one day to include the entire end date
            var endOfDay = endDate.Value.Date.AddDays(1).AddTicks(-1);
            filteredOrders = filteredOrders.Where(o => o.OrderTime <= endOfDay);
        }

        // Convert to list
        var filteredList = filteredOrders.ToList();

        // Calculate total pages
        totalPages = (int)Math.Ceiling(filteredList.Count / (double)pageSize);
        if (totalPages == 0) totalPages = 1;

        // Ensure current page is valid
        if (currentPage > totalPages) currentPage = totalPages;
        if (currentPage < 1) currentPage = 1;

        // Apply pagination
        var pagedOrders = filteredList
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Update the observable collection
        Orders.Clear();
        foreach (var order in pagedOrders)
        {
            Orders.Add(order);
        }

        // Update pagination UI
        UpdatePaginationUI();
    }

    // Update pagination controls
    private void UpdatePaginationUI()
    {
        CurrentPageText.Text = $"{currentPage} of {totalPages}";

        // Enable/disable buttons based on current page
        PreviousPageButton.IsEnabled = currentPage > 1;
        NextPageButton.IsEnabled = currentPage < totalPages;
    }

    // Previous page button click
    private void PreviousPage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (currentPage > 1)
        {
            currentPage--;
            ApplyFiltersAndPagination();
        }
    }

    // Next page button click
    private void NextPage_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        if (currentPage < totalPages)
        {
            currentPage++;
            ApplyFiltersAndPagination();
        }
    }

    // Reset filters
    private void ResetFilters_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        startDate = null;
        endDate = null;
        isStartDateSelected = false;
        isEndDateSelected = false;
        StartDatePicker.SelectedDate = null;
        EndDatePicker.SelectedDate = null;
        currentPage = 1;
        ApplyFiltersAndPagination();
    }

    // View order details click handler
    private async void ViewOrderDetails_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button?.Tag is Guid orderId)
        {
            await ShowOrderDetailsDialog(orderId);
        }
    }

    // Show order details dialog
    private async Task ShowOrderDetailsDialog(Guid orderId)
    {
        try
        {
            // Fetch order details from API
            var orderDetails = await _orderService.GetOrderByIdAsync(orderId);

            if (orderDetails != null)
            {
                // Store current order for printing
                _currentOrderDetails = orderDetails;

                // Populate dialog with order information
                DialogOrderId.Text = $"{orderDetails.Id}";
                DialogOrderDate.Text = orderDetails.OrderTime.ToString("MMM dd, yyyy hh:mm tt");
                DialogTotalItems.Text = $"{orderDetails.OrderItems?.Count ?? 0} items";
                DialogSubTotal.Text = $"Rs. {orderDetails.SubTotal:N2}";
                DialogDiscount.Text = orderDetails.Discount > 0
                    ? $"- Rs. {orderDetails.Discount:N2}"
                    : "Rs. 0.00";
                DialogTotalAmount.Text = $"Rs. {orderDetails.TotalAmount:N2}";

                // Populate order items
                if (orderDetails.OrderItems != null && orderDetails.OrderItems.Count > 0)
                {
                    OrderItemsRepeater.ItemsSource = orderDetails.OrderItems;
                }
                else
                {
                    OrderItemsRepeater.ItemsSource = new List<OrderItems>();
                }

                // Show the dialog
                await OrderDetailsDialog.ShowAsync();
            }
            else
            {
                // Show error dialog if order details not found
                var errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = "Unable to load order details. Please try again.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
        catch (Exception ex)
        {
            // Show error dialog on exception
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = $"An error occurred: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

    // Print order handler
    private async void PrintOrder_Click(ContentDialog sender, ContentDialogButtonClickEventArgs args)
    {
        // Don't prevent dialog from closing - we'll handle it differently
        args.Cancel = false;

        if (_currentOrderDetails == null)
        {
            return;
        }

        // Close the current dialog first
        OrderDetailsDialog.Hide();

        // Wait a bit for dialog to close
        await Task.Delay(100);

        try
        {
            // Show loading dialog
            var loadingDialog = new ContentDialog
            {
                Title = "Generating PDF",
                Content = new StackPanel
                {
                    Spacing = 10,
                    Children =
                    {
                        new ProgressRing { IsActive = true, Width = 50, Height = 50 },
                        new TextBlock { Text = "Please wait...", HorizontalAlignment = Microsoft.UI.Xaml.HorizontalAlignment.Center }
                    }
                },
                XamlRoot = this.XamlRoot
            };

            // Show loading dialog in background
            var showTask = loadingDialog.ShowAsync();

            // Generate PDF
            string pdfPath = await _printService.GenerateOrderPdf(
                _currentOrderDetails,
                _currentOrderDetails.OrderItems != null ? _currentOrderDetails.OrderItems.ToList() : new List<OrderItems>()
            );

            // Close loading dialog
            loadingDialog.Hide();

            // Wait for dialog to close
            await Task.Delay(100);

            // Open the PDF
            await _printService.OpenPdfAsync(pdfPath);

        }
        catch (Exception ex)
        {
            // Wait a bit to ensure previous dialog is closed
            await Task.Delay(100);

            // Show error message
            var errorDialog = new ContentDialog
            {
                Title = "Error",
                Content = $"Failed to generate PDF: {ex.Message}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await errorDialog.ShowAsync();
        }
    }

}
