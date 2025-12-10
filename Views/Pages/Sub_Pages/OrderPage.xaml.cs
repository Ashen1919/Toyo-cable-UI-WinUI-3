using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class OrderPage : Page
{
    private readonly ProductServices _productService;
    private readonly CategoryServices _categoryService;
    private readonly OrderService _orderService;

    public ObservableCollection<Products> Products { get; set; }
    public ObservableCollection<Category> Categories { get; set; }
    public ObservableCollection<CartItem> CartItems { get; set; }

    private ObservableCollection<Products> _allProducts;

    private decimal _subTotal;
    private decimal _discount;
    private decimal _totalPayment;

    public OrderPage()
    {
        InitializeComponent();
        _productService = new ProductServices();
        _categoryService = new CategoryServices();
        _orderService = new OrderService();

        Products = new ObservableCollection<Products>();
        Categories = new ObservableCollection<Category>();
        CartItems = new ObservableCollection<CartItem>();

        _allProducts = new ObservableCollection<Products>(Products);

        LoadProducts();
        LoadCategories();
        CalculateTotals();

        if (Products.Count > 0)
        {
            _allProducts = new ObservableCollection<Products>(Products);
            Debug.WriteLine($"Backed up {_allProducts.Count} products");
        }
        else
        {
            Debug.WriteLine("WARNING: No products loaded!");
        }
    }

    // Load all products
    public async void LoadProducts()
    {
        var products = await _productService.GetProductsAsync();
        if (products != null)
        {
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
        }
    }


    // Load all categories
    public async void LoadCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync();
        if (categories != null)
        {
            Categories.Clear();
            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }
    }

    // Add product to cart
    private void addToCartButton_Click(object sender, RoutedEventArgs e)
    {
        // Get the button that was clicked
        var button = sender as Button;
        if (button == null)
        {
            System.Diagnostics.Debug.WriteLine("Button return as null");
            return;
        }

        // Get the product from the button's DataContext
        var product = button.DataContext as Products;
        if (product == null)
        {
            System.Diagnostics.Debug.WriteLine("Button return as null");
            return;
        }

        // Check if product is already in cart
        var existingItem = CartItems.FirstOrDefault(item => item.ProductId == product.Id);

        if (existingItem != null)
        {
            // Increase quantity if already in cart
            existingItem.Quantity++;
        }
        else
        {
            // Add new item to cart
            var cartItem = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Category = product.Category,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Quantity = 1
            };

            CartItems.Add(cartItem);
        }

        // Recalculate totals
        CalculateTotals();
    }

    // Increase quantity
    private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        var cartItem = button.DataContext as CartItem;
        if (cartItem != null)
        {
            cartItem.Quantity++;
            CalculateTotals();
        }
    }

    // Decrease quantity
    private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        var cartItem = button.DataContext as CartItem;
        if (cartItem != null)
        {
            if (cartItem.Quantity > 1)
            {
                cartItem.Quantity--;
                CalculateTotals();
            }
            else
            {
                // Remove item if quantity would be 0
                CartItems.Remove(cartItem);
                CalculateTotals();
            }
        }
    }

    // Remove item from cart
    private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        if (button == null) return;

        var cartItem = button.DataContext as CartItem;
        if (cartItem != null)
        {
            CartItems.Remove(cartItem);
            CalculateTotals();
        }
    }

    // Reset order
    private void ResetOrder_Click(object sender, RoutedEventArgs e)
    {
        CartItems.Clear();
        CalculateTotals();
    }

    // Calculate totals
    private void CalculateTotals()
    {
        _subTotal = CartItems.Sum(item => item.TotalPrice);

        // _discount = _subTotal > 1000 ? _subTotal * 0.10m : 0;

        _totalPayment = _subTotal;

        // change texts
        subTotalText.Text = $"Rs. {_subTotal:F2}";
        // discountText.Text = $"Rs. {_discount:F2}";
        totalPaymentText.Text = $"Rs. {_totalPayment:F2}";
    }

    // Place order
    private async void PlaceOrder_Click(object sender, RoutedEventArgs e)
    {
        if (CartItems.Count == 0)
        {
            // Show message that cart is empty
            ContentDialog dialog = new ContentDialog
            {
                Title = "Empty Cart",
                Content = "Please add items to cart before placing an order.",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await dialog.ShowAsync();
            return;
        }

        // Show confirmation dialog
        ContentDialog confirmDialog = new ContentDialog
        {
            Title = "Confirm Order",
            Content = $"Total Amount: Rs. {_totalPayment:F2}\n\nDo you want to place this order?",
            PrimaryButtonText = "Place Order",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        var result = await confirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                // order object
                var order = new Order
                {
                    Discount = _discount,
                    OrderItems = CartItems.Select(item => new OrderItems
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                    }).ToList()

                };

                var createOrder = await _orderService.CreateOrderAsync(order);

                if(createOrder != null)
                {
                    // Show success message
                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Order Placed",
                        Content = "Your order has been placed successfully!",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();

                    // Clear cart after successful order
                    CartItems.Clear();
                    CalculateTotals();
                }
                else
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Failed to place order. Please try again.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to place order: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }

    // search box implementation
    private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // Check if _allProducts is initialized
        if (_allProducts == null || _allProducts.Count == 0)
        {
            Debug.WriteLine("ERROR: _allProducts is null or empty!");

            // Try to initialize it now if Products has items
            if (Products.Count > 0)
            {
                _allProducts = new ObservableCollection<Products>(Products);
                Debug.WriteLine($"Re-initialized _allProducts with {_allProducts.Count} products");
            }
            else
            {
                return;
            }
        }

        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var searchTerm = sender.Text?.Trim() ?? "";

            Debug.WriteLine($"Searching for: '{searchTerm}'");
            Debug.WriteLine($"Total products available: {_allProducts.Count}");

            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                // Clear suggestions
                sender.ItemsSource = null;

                // Show ALL products when search is empty
                Debug.WriteLine("Search cleared - showing all products");
                Products.Clear();
                foreach (var product in _allProducts)
                {
                    Products.Add(product);
                }
                Debug.WriteLine($"Displayed {Products.Count} products");
            }
            else
            {
                // Case-insensitive search
                var lowerSearchTerm = searchTerm.ToLower();

                // Find matching products
                var filteredProducts = _allProducts.Where(p =>
                    (!string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(lowerSearchTerm)) ||
                    (!string.IsNullOrEmpty(p.Category) && p.Category.ToLower().Contains(lowerSearchTerm))
                ).ToList();

                Debug.WriteLine($"Found {filteredProducts.Count} matching products");

                // Set suggestions for dropdown
                var suggestions = filteredProducts.Select(p => p.Name).Take(10).ToList();
                sender.ItemsSource = suggestions;

                // Update the product grid
                Products.Clear();
                foreach (var product in filteredProducts)
                {
                    Products.Add(product);
                }

                Debug.WriteLine($"Displayed {Products.Count} filtered products");
            }
        }
    }

    private void SearchBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        // Set the text to the selected suggestion
        if (args.SelectedItem != null)
        {
            sender.Text = args.SelectedItem.ToString();
        }
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        var searchTerm = args.QueryText?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            // Show all products
            Products.Clear();
            foreach (var product in _allProducts)
            {
                Products.Add(product);
            }
        }
        else
        {
            var lowerSearchTerm = searchTerm.ToLower();

            var filteredProducts = _allProducts.Where(p =>
                (!string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(lowerSearchTerm)) ||
                (!string.IsNullOrEmpty(p.Category) && p.Category.ToLower().Contains(lowerSearchTerm))
            ).ToList();

            Products.Clear();
            foreach (var product in filteredProducts)
            {
                Products.Add(product);
            }
        }
    }

}