using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using Toyo_cable_UI.Services;
using Toyo_cable_UI.Models;

namespace Toyo_cable_UI.Views.Pages.Sub_Pages
{
    public sealed partial class CategoryPage : Page
    {
        private readonly CategoryServices _categoryService;
        public ObservableCollection<Category> Categories { get; set; }

        public CategoryPage()
        {
            this.InitializeComponent();
            _categoryService = new CategoryServices();
            Categories = new ObservableCollection<Category>();
            LoadCategories();
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

            StackPanel dialogContent = new StackPanel
            {
                Spacing = 16,
                Width = 300
            };

            TextBox categoryNameTextBox = new TextBox
            {
                Header = "Category Name",
                PlaceholderText = "Enter category name"
            };

            dialogContent.Children.Add(categoryNameTextBox);
            addCategoryDialog.Content = dialogContent;

            ContentDialogResult result = await addCategoryDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string categoryName = categoryNameTextBox.Text;

                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Please enter a category name.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                var newCategory = new Category
                {
                    Name = categoryName
                };

                try
                {
                    var createdCategory = await _categoryService.CreateCategoryAsync(newCategory);

                    if (createdCategory != null)
                    {
                        LoadCategories();

                        ContentDialog successDialog = new ContentDialog
                        {
                            Title = "Success",
                            Content = $"Category '{categoryName}' has been added successfully.",
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
                            Content = "Failed to add category.",
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
                        Content = $"Failed to add category: {ex.Message}",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
        // Edit category
        private async void EditCategory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var category = button?.Tag as Category;

            if (category == null)
            {
                System.Diagnostics.Debug.WriteLine("Category is null in Edit");
                return;
            }

            // Create edit dialog
            ContentDialog editDialog = new ContentDialog
            {
                Title = "Edit Category",
                PrimaryButtonText = "Update",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.XamlRoot
            };

            StackPanel dialogContent = new StackPanel { Spacing = 16 };

            TextBox categoryNameTextBox = new TextBox
            {
                Header = "Category Name",
                Text = category.Name
            };

            dialogContent.Children.Add(categoryNameTextBox);
            editDialog.Content = dialogContent;

            ContentDialogResult result = await editDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                string newName = categoryNameTextBox.Text;

                if (string.IsNullOrWhiteSpace(newName))
                {
                    ContentDialog errorDialog = new ContentDialog
                    {
                        Title = "Validation Error",
                        Content = "Please enter a category name.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                    return;
                }

                // Update category
                var updatedCategory = new Category
                {
                    Id = category.Id,
                    Name = newName
                };

                var result_update = await _categoryService.UpdateCategoryAsync(category.Id, updatedCategory);

                //debug
                System.Diagnostics.Debug.WriteLine(result_update);

                if (result_update != null)
                {
                    LoadCategories();

                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Category updated successfully.",
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
                        Content = "Failed to update category.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }

        // Delete category
        private async void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var category = button?.Tag as Category;

            if (category == null) return;

            // Confirm deletion
            ContentDialog confirmDialog = new ContentDialog
            {
                Title = "Confirm Delete",
                Content = $"Are you sure you want to delete '{category.Name}'?",
                PrimaryButtonText = "Delete",
                CloseButtonText = "Cancel",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            ContentDialogResult result = await confirmDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                bool isDeleted = await _categoryService.DeleteCategoryAsync(category.Id);

                if (isDeleted)
                {
                    LoadCategories();

                    ContentDialog successDialog = new ContentDialog
                    {
                        Title = "Success",
                        Content = "Category deleted successfully.",
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
                        Content = "Failed to delete category.",
                        CloseButtonText = "OK",
                        XamlRoot = this.XamlRoot
                    };
                    await errorDialog.ShowAsync();
                }
            }
        }
    }
}