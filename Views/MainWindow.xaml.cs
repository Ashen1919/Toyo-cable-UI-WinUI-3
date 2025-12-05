using Microsoft.UI.Xaml;
using Toyo_cable_UI.Views.Pages;

namespace Toyo_cable_UI
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            RootFrame.Navigate(typeof(LoginPage));
        }
    }
}
