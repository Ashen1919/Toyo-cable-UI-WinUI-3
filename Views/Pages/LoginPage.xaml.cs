using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Toyo_cable_UI.Views.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        private void loginBtn_Click(object sender, RoutedEventArgs e)
        {
            var userName = userNameBox.Text;
            var password = passwordBox.Password;

            // Validate input fields are not empty
            if(string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password))
            {
                ContentDialog dialog = new()
                {
                    Title = "Error",
                    Content = "Please Enter Username & Password.",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                _ = dialog.ShowAsync();
                userNameBox.Text = "";
                passwordBox.Password = "";
                return;
            }

            // Login logic
            if(userName == "admin" && password == "admin")
            {
                Frame.Navigate(typeof(HomePage));
            }
            else
            {
                ContentDialog dialog = new()
                {
                    Title = "Login Failed",
                    Content = "Invalid Username or Password",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                _ = dialog.ShowAsync();
                userNameBox.Text = "";
                passwordBox.Password = "";
            }
        }
    }
}
