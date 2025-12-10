using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.ObjectModel;
using System.IO;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class ProductPage : Page
{
    private readonly ProductServices _productService;
    private readonly CategoryServices _categoryService;
    private readonly CloudinaryService _cloudinaryService;

    public ObservableCollection<Products> Products { get; set; }
    public ObservableCollection<Category> Categories { get; set; }

    private StorageFile? selectedImageFile;
    private string? selectedCategoryId;

    // Filter and pagination properties
    private string? _currentFilterOn = null;
    private string? _currentFilterQuery = null;
    private string? _currentSortBy = null;
    private bool _isAscending = true;
    private int _currentPage = 1;
    private int _pageSize = 25;

    private System.Threading.CancellationTokenSource? _searchCancellationTokenSource;

    public ProductPage()
    {
        _productService = new ProductServices();
        _categoryService = new CategoryServices();
        _cloudinaryService = new CloudinaryService();

        Products = new ObservableCollection<Products>();
        Categories = new ObservableCollection<Category>();

        InitializeComponent();

        this.Loaded += ProductPage_Loaded;
    }

    private void ProductPage_Loaded(object sender, RoutedEventArgs e)
    {
        LoadCategories();
        LoadProducts();
    }

    // Load all categories
    private async void LoadCategories()
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

    // Load all products
    private async void LoadProducts()
    {
        if (_productService == null)
            return;

        var products = await _productService.GetProductsAsync(
            filterOn: _currentFilterOn,
            filterQuery: _currentFilterQuery,
            sortBy: _currentSortBy,
            isAscending: _isAscending,
            pageNumber: _currentPage,
            pageSize: _pageSize
        );

        if (products != null)
        {
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }

            // Update pagination buttons
            PreviousPageButton.IsEnabled = _currentPage > 1;
            NextPageButton.IsEnabled = products.Count == _pageSize; 
            CurrentPageText.Text = _currentPage.ToString();
        }
    }

    // Filter type changed
    private void FilterTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // check if combobox is null
        if (FilterTypeComboBox.SelectedItem == null)
        {
            System.Diagnostics.Debug.WriteLine("Combo Box return null value");
            return;
        }

        var selectedItem = (ComboBoxItem)FilterTypeComboBox.SelectedItem;
        var filterType = selectedItem?.Content?.ToString();

        if (filterType == "All")
        {
            _currentFilterOn = null;
            _currentFilterQuery = null;
        }
        else
        {
            _currentFilterOn = filterType;
            SearchTextBox.IsEnabled = true;
        }

        _currentPage = 1;
        LoadProducts();
    }

    // Search text changed
    private async void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Cancel previous search
        _searchCancellationTokenSource?.Cancel();
        _searchCancellationTokenSource = new System.Threading.CancellationTokenSource();
        var token = _searchCancellationTokenSource.Token;

        try
        {
            // Wait 500ms before searching (debounce)
            await System.Threading.Tasks.Task.Delay(500, token);

            // Check if still not cancelled after delay
            if (!token.IsCancellationRequested)
            {
                _currentFilterQuery = string.IsNullOrWhiteSpace(SearchTextBox.Text) ? null : SearchTextBox.Text;
                _currentPage = 1;
                LoadProducts();
            }
        }
        catch (System.Threading.Tasks.TaskCanceledException)
        {
            // Search was cancelled because user is still typing - this is expected
        }
        catch (Exception ex)
        {
            // Handle unexpected errors
            System.Diagnostics.Debug.WriteLine($"Search error: {ex.Message}");
        }
    }
    // Sort changed
    private void SortByComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // check if combobox is null
        if (SortByComboBox.SelectedItem == null)
        {
            System.Diagnostics.Debug.WriteLine("Combo Box return null value");
            return;
        }

        var selectedItem = (ComboBoxItem)SortByComboBox.SelectedItem;
        var sortOption = selectedItem?.Content?.ToString();

        switch (sortOption)
        {
            case "None":
                _currentSortBy = null;
                _isAscending = true;
                break;
            case "Name (A-Z)":
                _currentSortBy = "Name";
                _isAscending = true;
                break;
            case "Name (Z-A)":
                _currentSortBy = "Name";
                _isAscending = false;
                break;
        }

        _currentPage = 1;
        LoadProducts();
    }

    // Page size changed
    private void PageSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // check if combobox is null
        if (PageSizeComboBox.SelectedItem == null)
        {
            System.Diagnostics.Debug.WriteLine("Combo Box return null value");
            return;
        }

        var selectedItem = (ComboBoxItem)PageSizeComboBox.SelectedItem;
        if (selectedItem != null && int.TryParse(selectedItem.Content?.ToString(), out int pageSize))
        {
            _pageSize = pageSize;
            _currentPage = 1;
            LoadProducts();
        }
    }

    // Clear all filters
    private void ClearFilters_Click(object sender, RoutedEventArgs e)
    {
        FilterTypeComboBox.SelectedIndex = 0; 
        SearchTextBox.Text = "";
        SortByComboBox.SelectedIndex = 0; 
        PageSizeComboBox.SelectedIndex = 1; 

        _currentFilterOn = null;
        _currentFilterQuery = null;
        _currentSortBy = null;
        _isAscending = true;
        _currentPage = 1;
        _pageSize = 25;

        LoadProducts();
    }

    // Previous page
    private void PreviousPage_Click(object sender, RoutedEventArgs e)
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            LoadProducts();
        }
    }

    // Next page
    private void NextPage_Click(object sender, RoutedEventArgs e)
    {
        _currentPage++;
        LoadProducts();
    }

    private async void addProductBtn_Click(object sender, RoutedEventArgs e)
    {
        selectedImageFile = null;
        selectedCategoryId = null;
        string? selectedCategoryName = null;

        ContentDialog contentDialog = new ContentDialog()
        {
            Title = "Add New Product",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        ScrollViewer scrollViewer = new ScrollViewer { MaxHeight = 500 };
        StackPanel stackPanel = new StackPanel() { Spacing = 16, Width = 400 };

        TextBox productNameTextBox = new TextBox
        {
            Header = "Product Name",
            PlaceholderText = "Enter Product Name"
        };

        TextBlock categoryLabel = new TextBlock
        {
            Text = "Category",
            FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
            Margin = new Thickness(0, 0, 0, 4)
        };

        DropDownButton categoryDropDown = new DropDownButton
        {
            Content = "Select Category",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        MenuFlyout flyout = new MenuFlyout();

        foreach (var category in Categories)
        {
            MenuFlyoutItem categoryItem = new MenuFlyoutItem
            {
                Text = category.Name,
                Tag = category
            };

            categoryItem.Click += (s, args) =>
            {
                var item = s as MenuFlyoutItem;
                var cat = item.Tag as Category;

                categoryDropDown.Content = cat.Name;
                selectedCategoryId = cat.Id.ToString();
                selectedCategoryName = cat.Name;
            };

            flyout.Items.Add(categoryItem);
        }

        categoryDropDown.Flyout = flyout;

        NumberBox quantityNumberBox = new NumberBox
        {
            Header = "Quantity",
            PlaceholderText = "Enter quantity",
            Minimum = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            Value = 0
        };

        TextBox priceTextBox = new TextBox
        {
            Header = "Unit Price",
            PlaceholderText = "Enter price (e.g., 1500.00)"
        };

        TextBlock imageLabel = new TextBlock
        {
            Text = "Product Image",
            FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
            Margin = new Thickness(0, 0, 0, 4)
        };

        StackPanel imagePanel = new StackPanel { Spacing = 8 };
        Button uploadImageButton = new Button
        {
            Content = "Choose Image",
            HorizontalAlignment = HorizontalAlignment.Left
        };

        TextBlock selectedImageText = new TextBlock
        {
            Text = "No image selected",
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
            FontStyle = Windows.UI.Text.FontStyle.Italic
        };

        uploadImageButton.Click += async (s, args) =>
        {
            FileOpenPicker picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle((Application.Current as App)?._window);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.ViewMode = PickerViewMode.Thumbnail;

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                selectedImageFile = file;
                selectedImageText.Text = $"Selected: {file.Name}";
                selectedImageText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
        };

        imagePanel.Children.Add(uploadImageButton);
        imagePanel.Children.Add(selectedImageText);

        stackPanel.Children.Add(productNameTextBox);
        stackPanel.Children.Add(categoryLabel);
        stackPanel.Children.Add(categoryDropDown);
        stackPanel.Children.Add(quantityNumberBox);
        stackPanel.Children.Add(priceTextBox);
        stackPanel.Children.Add(imageLabel);
        stackPanel.Children.Add(imagePanel);

        scrollViewer.Content = stackPanel;
        contentDialog.Content = scrollViewer;

        ContentDialogResult result = await contentDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            string productName = productNameTextBox.Text;
            int quantity = (int)quantityNumberBox.Value;
            string price = priceTextBox.Text;

            // Validation...
            if (string.IsNullOrWhiteSpace(productName) ||
                string.IsNullOrWhiteSpace(selectedCategoryName) ||
                string.IsNullOrWhiteSpace(price))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Validation Error",
                    Content = "Please fill in all required fields.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (!decimal.TryParse(price, out decimal priceValue) || priceValue < 0)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Validation Error",
                    Content = "Please enter a valid price.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            // Upload image to Cloudinary
            string imageUrl = null;
            if (selectedImageFile != null)
            {
                // Show loading indicator
                ContentDialog loadingDialog = new ContentDialog
                {
                    Title = "Uploading...",
                    Content = "Please wait while we upload the image.",
                    XamlRoot = this.XamlRoot
                };

                var loadingTask = loadingDialog.ShowAsync();

                // Upload to cloud
                imageUrl = await _cloudinaryService.UploadImageAsync(selectedImageFile);

                loadingDialog.Hide();

                if (imageUrl == null)
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Upload Error",
                        Content = "Failed to upload image. Please try again.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }
            }

            // Create new product with Cloudinary URL
            var newProduct = new Products
            {
                Id = Guid.NewGuid(),
                Name = productName,
                Category = selectedCategoryName,
                Quantity = quantity,
                Price = priceValue,
                ImageUrl = imageUrl ?? "https://placehold.co/600x400"
            };

            try
            {
                var createdProduct = await _productService.CreateProductsAsync(newProduct);

                if (createdProduct != null)
                {
                    LoadProducts();

                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = $"Product '{productName}' has been added successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Failed to add product. Please try again.",
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
                    Content = $"Failed to add product: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }

    // Edit Product
    private async void EditProduct_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var product = button?.Tag as Products;

        if (product == null)
        {
            System.Diagnostics.Debug.WriteLine("Product is null in Edit");
            return;
        }

        selectedImageFile = null;
        string selectedCategoryName = product.Category;

        ContentDialog editDialog = new ContentDialog
        {
            Title = "Edit Product",
            PrimaryButtonText = "Update",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        ScrollViewer scrollViewer = new ScrollViewer { MaxHeight = 500 };
        StackPanel stackPanel = new StackPanel() { Spacing = 16, Width = 400 };

        TextBox productNameTextBox = new TextBox
        {
            Header = "Product Name",
            Text = product.Name
        };

        TextBlock categoryLabel = new TextBlock
        {
            Text = "Category",
            FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
            Margin = new Thickness(0, 0, 0, 4)
        };

        DropDownButton categoryDropDown = new DropDownButton
        {
            Content = product.Category,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        MenuFlyout flyout = new MenuFlyout();

        foreach (var category in Categories)
        {
            MenuFlyoutItem categoryItem = new MenuFlyoutItem
            {
                Text = category.Name,
                Tag = category
            };

            categoryItem.Click += (s, args) =>
            {
                var item = s as MenuFlyoutItem;
                var cat = item.Tag as Category;

                categoryDropDown.Content = cat.Name;
                selectedCategoryName = cat.Name;
            };

            flyout.Items.Add(categoryItem);
        }

        categoryDropDown.Flyout = flyout;

        NumberBox quantityNumberBox = new NumberBox
        {
            Header = "Quantity",
            Minimum = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            Value = product.Quantity
        };

        TextBox priceTextBox = new TextBox
        {
            Header = "Unit Price",
            Text = product.Price.ToString()
        };

        TextBlock imageLabel = new TextBlock
        {
            Text = "Product Image",
            FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
            Margin = new Thickness(0, 0, 0, 4)
        };

        StackPanel imagePanel = new StackPanel { Spacing = 8 };

        TextBlock currentImageText = new TextBlock
        {
            Text = $"Current: {Path.GetFileName(product.ImageUrl)}",
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
        };

        Button uploadImageButton = new Button
        {
            Content = "Change Image",
            HorizontalAlignment = HorizontalAlignment.Left
        };

        TextBlock selectedImageText = new TextBlock
        {
            Text = "No new image selected",
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray),
            FontStyle = Windows.UI.Text.FontStyle.Italic
        };

        uploadImageButton.Click += async (s, args) =>
        {
            FileOpenPicker picker = new FileOpenPicker();
            var hwnd = WindowNative.GetWindowHandle((Application.Current as App)?._window);
            InitializeWithWindow.Initialize(picker, hwnd);

            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".png");
            picker.ViewMode = PickerViewMode.Thumbnail;

            StorageFile file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                selectedImageFile = file;
                selectedImageText.Text = $"New: {file.Name}";
                selectedImageText.Foreground = new SolidColorBrush(Microsoft.UI.Colors.Green);
            }
        };

        imagePanel.Children.Add(currentImageText);
        imagePanel.Children.Add(uploadImageButton);
        imagePanel.Children.Add(selectedImageText);

        stackPanel.Children.Add(productNameTextBox);
        stackPanel.Children.Add(categoryLabel);
        stackPanel.Children.Add(categoryDropDown);
        stackPanel.Children.Add(quantityNumberBox);
        stackPanel.Children.Add(priceTextBox);
        stackPanel.Children.Add(imageLabel);
        stackPanel.Children.Add(imagePanel);

        scrollViewer.Content = stackPanel;
        editDialog.Content = scrollViewer;

        ContentDialogResult result = await editDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            string productName = productNameTextBox.Text;
            int quantity = (int)quantityNumberBox.Value;
            string price = priceTextBox.Text;

            if (string.IsNullOrWhiteSpace(productName) ||
                string.IsNullOrWhiteSpace(selectedCategoryName) ||
                string.IsNullOrWhiteSpace(price))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Validation Error",
                    Content = "Please fill in all required fields.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            if (!decimal.TryParse(price, out decimal priceValue) || priceValue < 0)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Validation Error",
                    Content = "Please enter a valid price.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            string imagePath = product.ImageUrl; 
            if (selectedImageFile != null)
            {
                // Show loading indicator
                ContentDialog loadingDialog = new ContentDialog
                {
                    Title = "Uploading...",
                    Content = "Please wait while we upload the image.",
                    XamlRoot = this.XamlRoot
                };

                var loadingTask = loadingDialog.ShowAsync();

                // Upload to cloud
                imagePath = await _cloudinaryService.UploadImageAsync(selectedImageFile);

                loadingDialog.Hide();
            }

            var updatedProduct = new Products
            {
                Id = product.Id,
                Name = productName,
                Category = selectedCategoryName,
                Quantity = quantity,
                Price = priceValue,
                ImageUrl = imagePath ?? product.ImageUrl
            };

            try
            {
                var result_update = await _productService.UpdateProductAsync(product.Id, updatedProduct);

                if (result_update != null)
                {
                    LoadProducts();

                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Product updated successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
                else
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Error",
                        Content = "Failed to update product.",
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
                    Content = $"Failed to update product: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }

    // Delete Product
    private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
    {
        var button = sender as Button;
        var product = button?.Tag as Products;

        if (product == null) return;

        ContentDialog confirmDialog = new ContentDialog
        {
            Title = "Confirm Delete",
            Content = $"Are you sure you want to delete '{product.Name}'?",
            PrimaryButtonText = "Delete",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = this.XamlRoot
        };

        ContentDialogResult result = await confirmDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            try
            {
                // Delete from database
                bool isDeleted = await _cloudinaryService.DeleteImageAsync(product.ImageUrl);

                if (isDeleted)
                {
                    // Delete image from Cloudinary
                    if (!string.IsNullOrEmpty(product.ImageUrl))
                    {
                        await _productService.DeleteProductAsync(product.Id);
                    }

                    LoadProducts();

                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Product deleted successfully.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await successDialog.ShowAsync();
                }
            }
            catch (Exception ex)
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Failed to delete product: {ex.Message}",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
            }
        }
    }
}