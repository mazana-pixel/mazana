using System;
using System.Windows;
using System.Windows.Controls;

namespace MovieRecommenderApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // Frame to hold the sign-in and sign-up pages
        private Frame authFrame;

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Event handler for Sign In button click
        /// </summary>
        private void SignInButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new window to host the sign-in page
            Window signInWindow = new Window
            {
                Title = "Sign In",
                Width = 500,
                Height = 600,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Create frame to host the sign-in page
            authFrame = new Frame();
            signInWindow.Content = authFrame;

            // Navigate to the sign-in page
            authFrame.Navigate(new mazanabe.SignInPage());

            // Show the sign-in window as a dialog
            signInWindow.ShowDialog();
        }

        /// <summary>
        /// Event handler for Sign Up button click
        /// </summary>
        private void SignUpButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a new window to host the sign-up page
            Window signUpWindow = new Window
            {
                Title = "Sign Up",
                Width = 500,
                Height = 650,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Owner = this
            };

            // Create frame to host the sign-up page
            authFrame = new Frame();
            signUpWindow.Content = authFrame;

            // Navigate to the sign-up page
            authFrame.Navigate(new mazanabe.SignUpPage());

            // Show the sign-up window as a dialog
            signUpWindow.ShowDialog();
        }

        /// <summary>
        /// Event handler for Explore Movies button click
        /// </summary>
        private void ExploreButton_Click(object sender, RoutedEventArgs e)
        {
            // Check if user is authenticated
            if (mazanabe.UserData.IsAuthenticated)
            {
                // Navigate to movie exploration page
                var movieExplorer = new MovieExplorer();
                movieExplorer.Owner = this;
                movieExplorer.Show();
                this.Hide();
            }
            else
            {
                // Prompt user to sign in first
                MessageBox.Show("Please sign in to explore movies.",
                    "Authentication Required",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);

                SignInButton_Click(sender, e);
            }
        }
    }

    /// <summary>
    /// Simple movie explorer window
    /// </summary>
    public class MovieExplorer : Window
    {
        public MovieExplorer()
        {
            Title = "Movie Explorer";
            Width = 800;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = System.Windows.Media.Brushes.Black;

            var grid = new Grid();
            Content = grid;

            var stackPanel = new StackPanel
            {
                Margin = new Thickness(20)
            };
            grid.Children.Add(stackPanel);

            // Welcome message
            stackPanel.Children.Add(new TextBlock
            {
                Text = $"Welcome, {mazanabe.UserData.CurrentUser}!",
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Simple message
            stackPanel.Children.Add(new TextBlock
            {
                Text = "This is a simple placeholder for the movie explorer screen. In a real application, this would show personalized movie recommendations based on user preferences.",
                Foreground = System.Windows.Media.Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 20)
            });

            // Back button
            var backButton = new Button
            {
                Content = "Back to Home",
                Padding = new Thickness(10, 5, 10, 5),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            backButton.Click += BackButton_Click;
            stackPanel.Children.Add(backButton);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Show the main window and close this one
            if (Owner != null)
            {
                Owner.Show();
            }
            this.Close();
        }

        protected override void OnClosed(EventArgs e)
        {
            // Make sure main window is shown when this window is closed
            if (Owner != null && !Owner.IsVisible)
            {
                Owner.Show();
            }
            base.OnClosed(e);
        }
    }
}