using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace mazanabe
{
    /// <summary>
    /// Interaction logic for SignInPage.xaml
    /// </summary>
    public partial class SignInPage : Page
    {
        public SignInPage()
        {
            InitializeComponent();
        }

        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both email and password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // In a real application, you would validate credentials against a database
            // For now, we'll just simulate a successful login
            MessageBox.Show("Sign in successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Store user authentication state
            UserData.IsAuthenticated = true;
            UserData.CurrentUser = email;

            // Close the sign in window
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }

        private void ForgotPasswordButton_Click(object sender, MouseButtonEventArgs e)
        {
            MessageBox.Show("Password reset feature will be implemented in future updates.",
                "Feature Coming Soon", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SignUpButton_Click(object sender, MouseButtonEventArgs e)
        {
            // Find the parent window's frame and navigate to the Sign Up page
            Frame parentFrame = this.Parent as Frame;
            if (parentFrame != null)
            {
                parentFrame.Navigate(new SignUpPage());
            }
        }
    }

    // If not already defined elsewhere in your project, add this class
    public static class UserData
    {
        public static bool IsAuthenticated { get; set; } = false;
        public static string CurrentUser { get; set; } = string.Empty;
    }
}