using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Windows.Storage;
using Windows.Storage.Pickers;
using WinRT.Interop;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class ProductPage : Page
{
    private StorageFile selectedImageFile;

    public ProductPage()
    {
        InitializeComponent();
    }

    private async void addProductBtn_Click(object sender, RoutedEventArgs e)
    {
        ContentDialog contentDialog = new ContentDialog()
        {
            Title = "Add New Product",
            PrimaryButtonText = "Save",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Primary,
            XamlRoot = this.XamlRoot
        };

        // creating form content with ScrollViewer for better UX
        ScrollViewer scrollViewer = new ScrollViewer
        {
            MaxHeight = 500
        };

        StackPanel stackPanel = new StackPanel()
        {
            Spacing = 16,
            Width = 400
        };

        // Product ID TextBox
        TextBox productIdTextBox = new TextBox
        {
            Header = "Product ID",
            PlaceholderText = "Enter Product ID"
        };

        // Product Name TextBox
        TextBox productNameTextBox = new TextBox
        {
            Header = "Product Name",
            PlaceholderText = "Enter Product Name"
        };

        // Category Label
        TextBlock categoryLabel = new TextBlock
        {
            Text = "Category",
            FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
            Margin = new Thickness(0, 0, 0, 4)
        };

        // Category DropDownButton
        DropDownButton categoryDropDown = new DropDownButton
        {
            Content = "Select Category",
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        // Create MenuFlyout for dropdown
        MenuFlyout flyout = new MenuFlyout();

        // Add menu item
        MenuFlyoutItem categoryItem = new MenuFlyoutItem
        {
            Text = "Accelerator Cable"
        };
        categoryItem.Click += (s, args) =>
        {
            categoryDropDown.Content = "Accelerator Cable";
        };

        // Add item to flyout
        flyout.Items.Add(categoryItem);

        // Attach flyout to dropdown
        categoryDropDown.Flyout = flyout;

        // Quantity NumberBox
        NumberBox quantityNumberBox = new NumberBox
        {
            Header = "Quantity",
            PlaceholderText = "Enter quantity",
            Minimum = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Inline,
            Value = 0
        };

        // Price TextBox (formatted for currency)
        TextBox priceTextBox = new TextBox
        {
            Header = "Price",
            PlaceholderText = "Enter price (e.g., 1500.00)"
        };

        // Image Upload Section
        TextBlock imageLabel = new TextBlock
        {
            Text = "Product Image",
            FontWeight = new Windows.UI.Text.FontWeight { Weight = 600 },
            Margin = new Thickness(0, 0, 0, 4)
        };

        // Image upload button and preview
        StackPanel imagePanel = new StackPanel
        {
            Spacing = 8
        };

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

            // Get the window handle for the picker
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

        // Add all controls to stack panel
        stackPanel.Children.Add(productIdTextBox);
        stackPanel.Children.Add(productNameTextBox);
        stackPanel.Children.Add(categoryLabel);
        stackPanel.Children.Add(categoryDropDown);
        stackPanel.Children.Add(quantityNumberBox);
        stackPanel.Children.Add(priceTextBox);
        stackPanel.Children.Add(imageLabel);
        stackPanel.Children.Add(imagePanel);

        // Set scrollviewer content
        scrollViewer.Content = stackPanel;

        // Set dialog content
        contentDialog.Content = scrollViewer;

        // Show dialog and handle result
        ContentDialogResult result = await contentDialog.ShowAsync();

        if (result == ContentDialogResult.Primary)
        {
            // Get values
            string productId = productIdTextBox.Text;
            string productName = productNameTextBox.Text;
            string category = categoryDropDown.Content?.ToString();
            double quantity = quantityNumberBox.Value;
            string price = priceTextBox.Text;
            string imagePath = selectedImageFile?.Path ?? "No image";

            // Validate
            if (string.IsNullOrWhiteSpace(productId) ||
                string.IsNullOrWhiteSpace(productName) ||
                category == "Select Category" ||
                string.IsNullOrWhiteSpace(price))
            {
                ContentDialog errorDialog = new ContentDialog
                {
                    Title = "Validation Error",
                    Content = "Please fill in all required fields (Product ID, Name, Category, and Price).",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await errorDialog.ShowAsync();
                return;
            }

            // Validate price format
            if (!double.TryParse(price, out double priceValue) || priceValue < 0)
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

            // TODO: Save to database
            ContentDialog successDialog = new ContentDialog
            {
                Title = "Success",
                Content = $"Product '{productName}' has been added successfully.\n" +
                          $"Quantity: {quantity}\n" +
                          $"Price: {priceValue:C}\n" +
                          $"Image: {(selectedImageFile != null ? selectedImageFile.Name : "None")}",
                CloseButtonText = "OK",
                XamlRoot = this.XamlRoot
            };
            await successDialog.ShowAsync();

            // Reset selected image for next use
            selectedImageFile = null;
        }
    }
}