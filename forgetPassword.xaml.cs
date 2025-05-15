using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SignInSignUpApp; // Import the namespace containing AuthenticationService

namespace mazanabe
{
    public partial class ForgotPasswordPage : Page
    {
        public ForgotPasswordPage()
        {
            InitializeComponent();

            // Set focus to the email textbox when the page loads
            Loaded += (s, e) => {
                EmailTextBox.Focus();
            };
        }

        private void ResetPasswordButton_Click(object sender, RoutedEventArgs e)
        {
            string email = EmailTextBox.Text?.Trim();

            // Validate email
            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Please enter your email address.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EmailTextBox.Focus();
                return;
            }

            if (AuthenticationService.ResetPassword(email))
            {
                MessageBox.Show("Password reset instructions have been sent to your email.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                NavigationService.Navigate(new SignInPage());
            }
            else
            {
                MessageBox.Show("Email not found in our system.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                EmailTextBox.Focus();
                EmailTextBox.SelectAll();
            }
        }

        private void BackToSignIn_Click(object sender, MouseButtonEventArgs e)
        {
            NavigationService.Navigate(new SignInPage());
        }
    }
}
