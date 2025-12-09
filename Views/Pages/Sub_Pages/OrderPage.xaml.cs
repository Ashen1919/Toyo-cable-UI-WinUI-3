using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Toyo_cable_UI.Models;
using Toyo_cable_UI.Services;


namespace Toyo_cable_UI.Views.Pages.Sub_Pages;

public sealed partial class OrderPage : Page
{
    private readonly ProductServices _productService;
    private readonly CategoryServices _categoryService;

    public ObservableCollection<Products> Products { get; set; }
    public ObservableCollection<Category> Categories { get; }

    public OrderPage()
    {
        InitializeComponent();

        _productService = new ProductServices();
        _categoryService = new CategoryServices();

        Products = new ObservableCollection<Products>();
        Categories = new ObservableCollection<Category>();

        LoadProducts();
        LoadCategories();
    }

    //load all products
    public async void LoadProducts()
    {
        var products = await _productService.GetProductsAsync();

        if(products != null)
        {
            Products.Clear();

            foreach(var product in products)
            {
                Products.Add(product);
            }
        }
    }

    // Load all categories
    public async void LoadCategories()
    {
        var categories = await _categoryService.GetCategoriesAsync();

        if(categories != null)
        {
            Categories.Clear();

            foreach(var category in categories)
            {
                Categories.Add(category);
            }
        }
    }

}
