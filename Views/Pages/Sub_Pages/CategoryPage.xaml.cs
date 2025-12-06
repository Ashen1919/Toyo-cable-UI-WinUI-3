using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages
{
    public sealed partial class CategoryPage : Page
    {
        public CategoryPage()
        {
            this.InitializeComponent();
        }

        private async void AddCategoryButton_Click(object sender, RoutedEventArgs e)
        {
            // Create the dialog
            ContentDialog addCategoryDialog = new ContentDialog
            {
                Title = "Add New Category",
                PrimaryButtonText = "Save",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            // Create the form content
            StackPanel dialogContent = new StackPanel
            {
                Spacing = 16
            };

            // Category ID TextBox
            TextBox categoryIdTextBox = new TextBox
            {
                Header = "Category ID",
                PlaceholderText = "Enter category ID (e.g., CG002)"
            };

            // Category Name TextBox
            TextBox categoryNameTextBox = new TextBox
            {
                Header = "Category Name",
                PlaceholderText = "Enter category name"
            };

            // Add controls to dialog content
            dialogContent.Children.Add(categoryIdTextBox);
            dialogContent.Children.Add(categoryNameTextBox);

            // Set the dialog content
            addCategoryDialog.Content = dialogContent;

            // Show the dialog and handle the result
            ContentDialogResult result = await addCategoryDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                // User clicked Save button
                string categoryId = categoryIdTextBox.Text;
                string categoryName = categoryNameTextBox.Text;

                // Validate input
                if (string.IsNullOrWhiteSpace(categoryId) || string.IsNullOrWhiteSpace(categoryName))
                {
                    // Show error message
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Please fill in all fields.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                // TODO: Add your logic to save the category to database
                // For now, just show a success message
                ContentDialog successDialog = new ContentDialog
                {
                    Title = "Success",
                    Content = $"Category '{categoryName}' with ID '{categoryId}' has been added successfully.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };
                await successDialog.ShowAsync();
            }
        }
    }
}