using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Navigation;

namespace mazanabe
{
    /// <summary>
    /// Interaction logic for SignUpPage.xaml
    /// </summary>
    public partial class SignUpPage : Page
    {
        public SignUpPage()
        {
            InitializeComponent();
        }

        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            string name = NameTextBox.Text;
            string email = EmailTextBox.Text;
            string password = PasswordBox.Password;

            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("All fields are required.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // In a real application, you would validate the email format and password strength
            // and create an account in your database

            // For now, just show success message
            MessageBox.Show("Account created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Automatically log the user in
            UserData.IsAuthenticated = true;
            UserData.CurrentUser = email;

            // Close the sign up window
            Window parentWindow = Window.GetWindow(this);
            if (parentWindow != null)
            {
                parentWindow.Close();
            }
        }

        private void SignInButton_Click(object sender, MouseButtonEventArgs e)
        {
            // Find the parent window's frame and navigate to the Sign In page
            Frame parentFrame = this.Parent as Frame;
            if (parentFrame != null)
            {
                parentFrame.Navigate(new SignInPage());
            }
        }
    }
}