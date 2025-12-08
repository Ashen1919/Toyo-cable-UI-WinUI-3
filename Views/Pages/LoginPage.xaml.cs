using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Threading.Tasks;
using Toyo_cable_UI.Services;

namespace Toyo_cable_UI.Views.Pages
{
    public sealed partial class LoginPage : Page
    {
        private readonly AuthService _authService;

        public LoginPage()
        {
            this.InitializeComponent();
            _authService = new AuthService();
        }

        // Enable/Disable login button based on input
        private void InputTextBox_TextChanged(object sender, object e)
        {
            LoginButton.IsEnabled = !string.IsNullOrWhiteSpace(emailTextBox.Text) &&
                                     !string.IsNullOrWhiteSpace(passwordBox.Password);

            // Hide error when user types
            ErrorInfoBar.IsOpen = false;
        }

        // Handle login button click
        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            await PerformLogin();
        }

        // Handle Enter key press in password box
        private async void PasswordBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && LoginButton.IsEnabled)
            {
                await PerformLogin();
                e.Handled = true;
            }
        }

        // Main login logic
        private async Task PerformLogin()
        {
            // Show loading, disable input
            SetLoadingState(true);

            try
            {
                var email = emailTextBox.Text.Trim();
                var password = passwordBox.Password;

                // Call API
                var (success, message, user) = await _authService.LoginAsync(email, password);

                if (success)
                {
                    // Navigate to dashboard
                    Frame.Navigate(typeof(HomePage), user);
                }
                else
                {
                    // Show error
                    ShowError(message);

                    // Clear password on failure
                    passwordBox.Password = string.Empty;
                }
            }
            catch (Exception ex)
            {
                ShowError($"Unexpected error: {ex.Message}");
            }
            finally
            {
                // Hide loading, enable input
                SetLoadingState(false);
            }
        }

        // Show/Hide loading state
        private void SetLoadingState(bool isLoading)
        {
            LoginButton.IsEnabled = !isLoading;
            emailTextBox.IsEnabled = !isLoading;
            passwordBox.IsEnabled = !isLoading;
            LoadingPanel.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            LoginButton.Content = isLoading ? "" : "Login"; // Hide text when loading
        }

        // Show error message
        private void ShowError(string message)
        {
            ErrorInfoBar.Message = message;
            ErrorInfoBar.IsOpen = true;
        }
    }
}
