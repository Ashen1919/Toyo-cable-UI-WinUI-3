using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class viewOrderPage : Page
{
    private readonly OrderService _orderService;
    public ObservableCollection<Order> Orders { get; set; }

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

    public viewOrderPage()
    {
        InitializeComponent();
        _orderService = new OrderService();
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

    // Optional: Reset filters
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
}