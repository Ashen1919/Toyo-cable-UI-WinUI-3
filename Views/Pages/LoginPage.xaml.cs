using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;


namespace Toyo_cable_UI.Views.Pages
{
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

        // Trigger with enter key
        private void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if(e.Key == Windows.System.VirtualKey.Enter)
            {
                loginBtn_Click(sender, new RoutedEventArgs());
                e.Handled = true;
            }
        }
    }
}
