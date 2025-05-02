using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace mazanabe
{
    public class Movie
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public string IMDbRating { get; set; }
    }

    public class MovieRecommendation
    {
        public string Title { get; set; }
        public string Year { get; set; }
        public string Genre { get; set; }
        public string Rating { get; set; }
        public string Description { get; set; }
    }

    public partial class MainWindow : Window
    {
        private const string ConnectionString = "Server=127.0.0.1;Uid=root;Database=mazana;Password=new_password;AllowPublicKeyRetrieval=true;SslMode=none;";
        private ObservableCollection<Movie> favorites = new ObservableCollection<Movie>();
        private ObservableCollection<Movie> watchlist = new ObservableCollection<Movie>();
        private int userPoints = 0;
        private int currentUserId = 1; // Default user ID

        public MainWindow()
        {
            InitializeComponent();

            // Set data contexts for ListViews
            FavoritesListView.ItemsSource = favorites;
            WatchlistListView.ItemsSource = watchlist;

            LoadUserPoints();
            LoadFavorites();
            LoadWatchlist();
        }

        private void GoToAdminDashboard_Click(object sender, RoutedEventArgs e)
        {
            AdminDashboard adminDashboardWindow = new AdminDashboard();
            adminDashboardWindow.Show();
        }

        private void LoadUserPoints()
        {
            try
            {
                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "SELECT points FROM users WHERE ID = @userId";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", currentUserId);
                        object result = command.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            userPoints = Convert.ToInt32(result);
                            UserPointsDisplay.Text = userPoints.ToString();
                        }
                        else
                        {
                            // If no points found, initialize with default points
                            userPoints = 0;
                            UserPointsDisplay.Text = "0";

                            // Create default points entry if needed
                            string insertQuery = "INSERT INTO users (ID, points) VALUES (@userId, 0) ON DUPLICATE KEY UPDATE points = 0";
                            using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                            {
                                insertCommand.Parameters.AddWithValue("@userId", currentUserId);
                                insertCommand.ExecuteNonQuery();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading user points: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                UserPointsDisplay.Text = "0";
            }
        }

        private void UpdateUserPoints(int pointsToAdd)
        {
            try
            {
                userPoints += pointsToAdd;
                UserPointsDisplay.Text = userPoints.ToString();

                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    string query = "UPDATE users SET points = @points WHERE ID = @userId";
                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@points", userPoints);
                        command.Parameters.AddWithValue("@userId", currentUserId);
                        command.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating user points: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                // Revert the points increase in the UI
                userPoints -= pointsToAdd;
                UserPointsDisplay.Text = userPoints.ToString();
            }
        }

        private void LoadFavorites()
        {
            try
            {
                favorites.Clear();

                using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    // Updated to match the database schema
                    string query = @"
                        SELECT m.Title, SUBSTRING(m.ReleaseDate, 1, 4) AS Year, m.Genre, m.IMDbRating
                        FROM favourites f
                        JOIN movies m ON f.movie_id = m.ID
                        WHERE f.user_id = @userId";

                    using (MySqlCommand command = new MySqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@userId", currentUserId);
                        using (MySqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string movieTitle = reader["Title"].ToString();
                                string movieYear = reader["Year"] != DBNull.Value ? reader["Year"].ToString() : "N/A";
                                string movieGenre = reader["Genre"] != DBNull.Value ? reader["Genre"].ToString() : "N/A";
                                string movieRating = reader["IMDbRating"] != DBNull.Value ? reader["IMDbRating"].ToString() : "N/A";

                                favorites.Add(new Movie
                                {
                                    Title = movieTitle,
                                    Year = movieYear,
                                    Genre = movieGenre,
                                    IMDbRating = movieRating
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading favorites: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadWatchlist()
        {
            try
            {
                watchlist.Clear();

                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();
                    // Updated to match the database schema - using the watchlist table
                    string query = @"
                        SELECT m.Title, SUBSTRING(m.ReleaseDate, 1, 4) AS Year, m.Genre, m.IMDbRating
                        FROM watchlist w
                        JOIN movies m ON w.movie_id = m.ID
                        WHERE w.user_id = @userId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string title = reader["Title"].ToString();
                                string year = reader["Year"].ToString();
                                string genre = reader["Genre"] != DBNull.Value ? reader["Genre"].ToString() : "N/A";
                                string rating = reader["IMDbRating"] != DBNull.Value ? reader["IMDbRating"].ToString() : "N/A";

                                watchlist.Add(new Movie
                                {
                                    Title = title,
                                    Year = year,
                                    Genre = genre,
                                    IMDbRating = rating
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading watchlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            LoadRecommendations();
            UpdateUserPoints(10); // Add 10 points whenever recommendations are generated
        }

        private void LoadRecommendations()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                {
                    conn.Open();

                    // Build the recommendation query based on user selections
                    List<string> selectedGenres = new List<string>();
                    foreach (ListViewItem item in MovieTypeList.Items)
                    {
                        if (item.Content is CheckBox checkBox && checkBox.IsChecked == true)
                        {
                            // Extract just the genre name without extra spaces
                            string genreName = checkBox.Content.ToString().Trim();
                            selectedGenres.Add($"'{genreName}'");
                        }
                    }

                    string selectedEra = "";
                    foreach (ListViewItem item in MovieEraList.Items)
                    {
                        if (item.Content is RadioButton radioButton && radioButton.IsChecked == true)
                        {
                            selectedEra = radioButton.Content.ToString();
                            break;
                        }
                    }

                    string imdbScoreThreshold = "";
                    if (RatingImportanceCombo.SelectedItem is ComboBoxItem ratingItem)
                    {
                        if (ratingItem.Content.ToString().Contains("(8.0+)"))
                        {
                            imdbScoreThreshold = "AND m.IMDbRating >= 8.0";
                        }
                        else if (ratingItem.Content.ToString().Contains("(7.0+)"))
                        {
                            imdbScoreThreshold = "AND m.IMDbRating >= 7.0";
                        }
                        // "Not Important" means no threshold
                    }

                    // Handle genres with LIKE because genre field may contain multiple genres
                    string genreCondition = "";
                    if (selectedGenres.Any())
                    {
                        List<string> likeConditions = new List<string>();
                        foreach (string genre in selectedGenres)
                        {
                            // Remove the quotes from the genre string for the LIKE clause
                            string genreWithoutQuotes = genre.Trim('\'');
                            likeConditions.Add($"m.Genre LIKE '%{genreWithoutQuotes}%'");
                        }
                        genreCondition = $"AND ({string.Join(" OR ", likeConditions)})";
                    }

                    string eraCondition = "";
                    switch (selectedEra)
                    {
                        case "Classic (Pre-1970s)":
                            eraCondition = "AND CAST(SUBSTRING(m.ReleaseDate, 1, 4) AS UNSIGNED) < 1970";
                            break;
                        case "Retro (1970s-1990s)":
                            eraCondition = "AND CAST(SUBSTRING(m.ReleaseDate, 1, 4) AS UNSIGNED) >= 1970 AND CAST(SUBSTRING(m.ReleaseDate, 1, 4) AS UNSIGNED) <= 1999";
                            break;
                        case "Modern (2000s-2010s)":
                            eraCondition = "AND CAST(SUBSTRING(m.ReleaseDate, 1, 4) AS UNSIGNED) >= 2000 AND CAST(SUBSTRING(m.ReleaseDate, 1, 4) AS UNSIGNED) <= 2019";
                            break;
                        case "Contemporary (2020s+)":
                            eraCondition = "AND CAST(SUBSTRING(m.ReleaseDate, 1, 4) AS UNSIGNED) >= 2020";
                            break;
                        case "All Eras":
                        default:
                            break;
                    }

                    // Enhance algorithm by:
                    // 1. Weighting by IMDb score
                    // 2. Excluding movies already in favorites or watchlist
                    // 3. Including more information about the movie
                    string query = $@"
                        SELECT 
                            m.Title, 
                            SUBSTRING(m.ReleaseDate, 1, 4) AS Year, 
                            m.Genre, 
                            CONCAT(m.IMDbRating, '/10') AS Rating,
                            m.PlotSummary AS Description,
                            m.ID as MovieID
                        FROM movies m
                        LEFT JOIN favourites f ON m.ID = f.movie_id AND f.user_id = @userId
                        LEFT JOIN watchlist w ON m.ID = w.movie_id AND w.user_id = @userId
                        WHERE 1=1 -- Base condition
                        {genreCondition}
                        {eraCondition}
                        {imdbScoreThreshold}
                        AND f.id IS NULL -- Not in favorites
                        AND w.id IS NULL -- Not in watchlist
                        ORDER BY m.IMDbRating DESC, RAND() -- Order by rating then randomize
                        LIMIT 5";


                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", currentUserId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            // Clear existing recommendations
                            RecommendationsPanel.Children.Clear();
                            RecommendationBoxPlaceholder.Visibility = Visibility.Collapsed;

                            if (!reader.HasRows)
                            {
                                TextBlock noRecommendations = new TextBlock
                                {
                                    Text = "No recommendations found based on your preferences. Try selecting different criteria.",
                                    Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                                    Margin = new Thickness(5),
                                    TextWrapping = TextWrapping.Wrap,
                                    FontSize = 14
                                };
                                RecommendationsPanel.Children.Add(noRecommendations);
                            }
                            else
                            {
                                while (reader.Read())
                                {
                                    string title = reader["Title"].ToString();
                                    string year = reader["Year"].ToString();
                                    string genre = reader["Genre"].ToString();
                                    string rating = reader["Rating"].ToString();
                                    string description = reader["Description"] != DBNull.Value
                                        ? reader["Description"].ToString()
                                        : "No description available.";
                                    int movieId = Convert.ToInt32(reader["MovieID"]);

                                    // Create a movie recommendation object
                                    MovieRecommendation movieRec = new MovieRecommendation
                                    {
                                        Title = title,
                                        Year = year,
                                        Genre = genre,
                                        Rating = rating,
                                        Description = description
                                    };

                                    // Create the border using the template in XAML
                                    Border recommendationBorder = new Border
                                    {
                                        Background = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                                        CornerRadius = new CornerRadius(8),
                                        Padding = new Thickness(12),
                                        Margin = new Thickness(0, 0, 0, 12)
                                    };

                                    Grid grid = new Grid();
                                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                                    // Movie Icon/Poster
                                    Border posterBorder = new Border
                                    {
                                        Width = 50,
                                        Height = 70,
                                        Background = new SolidColorBrush(Color.FromRgb(68, 68, 68)),
                                        CornerRadius = new CornerRadius(5),
                                        Margin = new Thickness(0, 0, 12, 0)
                                    };

                                    TextBlock posterText = new TextBlock
                                    {
                                        Text = "🎬",
                                        FontSize = 24,
                                        HorizontalAlignment = HorizontalAlignment.Center,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };

                                    posterBorder.Child = posterText;
                                    Grid.SetColumn(posterBorder, 0);
                                    grid.Children.Add(posterBorder);

                                    // Movie details
                                    StackPanel detailsPanel = new StackPanel();

                                    TextBlock titleBlock = new TextBlock
                                    {
                                        Text = movieRec.Title,
                                        FontWeight = FontWeights.Bold,
                                        FontSize = 16,
                                        Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224))
                                    };
                                    detailsPanel.Children.Add(titleBlock);

                                    TextBlock yearBlock = new TextBlock
                                    {
                                        Text = movieRec.Year,
                                        FontSize = 13,
                                        Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                                        Margin = new Thickness(0, 2, 0, 5)
                                    };
                                    detailsPanel.Children.Add(yearBlock);

                                    TextBlock genreBlock = new TextBlock
                                    {
                                        Text = movieRec.Genre,
                                        FontSize = 12,
                                        Foreground = new SolidColorBrush(Color.FromRgb(160, 160, 160))
                                    };
                                    detailsPanel.Children.Add(genreBlock);

                                    TextBlock ratingBlock = new TextBlock
                                    {
                                        Text = movieRec.Rating,
                                        FontSize = 12,
                                        Foreground = new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                                        Margin = new Thickness(0, 2, 0, 0)
                                    };
                                    detailsPanel.Children.Add(ratingBlock);

                                    Grid.SetColumn(detailsPanel, 1);
                                    grid.Children.Add(detailsPanel);

                                    // Buttons
                                    StackPanel buttonsPanel = new StackPanel
                                    {
                                        Orientation = Orientation.Vertical,
                                        VerticalAlignment = VerticalAlignment.Center
                                    };

                                    // Add to Favorites button
                                    Button addToFavButton = new Button
                                    {
                                        Content = "+ Favorite",
                                        Width = 80,
                                        Margin = new Thickness(0, 0, 0, 8),
                                        Tag = movieId // Store movie ID instead of title
                                    };
                                    addToFavButton.SetResourceReference(StyleProperty, "SmallActionButtonStyle");
                                    addToFavButton.Click += AddToFavorites_Click;
                                    buttonsPanel.Children.Add(addToFavButton);

                                    // Add to Watchlist button
                                    Button addToWatchlistButton = new Button
                                    {
                                        Content = "+ Watchlist",
                                        Width = 80,
                                        Tag = movieId // Store movie ID instead of title
                                    };
                                    addToWatchlistButton.SetResourceReference(StyleProperty, "SmallActionButtonStyle");
                                    addToWatchlistButton.Click += AddToWatchlist_Click;
                                    buttonsPanel.Children.Add(addToWatchlistButton);

                                    Grid.SetColumn(buttonsPanel, 2);
                                    grid.Children.Add(buttonsPanel);

                                    recommendationBorder.Child = grid;
                                    RecommendationsPanel.Children.Add(recommendationBorder);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading recommendations: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddToFavorites_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int movieId)
            {
                try
                {
                    using (MySqlConnection connection = new MySqlConnection(ConnectionString))
                    {
                        connection.Open();

                        // Check if movie already exists in favorites
                        string checkQuery = "SELECT COUNT(*) FROM favourites WHERE movie_id = @movieId AND user_id = @userId";
                        using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.AddWithValue("@movieId", movieId);
                            checkCommand.Parameters.AddWithValue("@userId", currentUserId);
                            long count = Convert.ToInt64(checkCommand.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show("This movie is already in your favorites!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                        }

                        // Get movie details for UI update
                        string detailsQuery = "SELECT Title, SUBSTRING(ReleaseDate, 1, 4) AS Year, Genre, IMDbRating FROM movies WHERE ID = @movieId";
                        string title = "";
                        string year = "N/A";
                        string genre = "N/A";
                        string rating = "N/A";

                        using (MySqlCommand detailsCommand = new MySqlCommand(detailsQuery, connection))
                        {
                            detailsCommand.Parameters.AddWithValue("@movieId", movieId);
                            using (MySqlDataReader reader = detailsCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    title = reader["Title"].ToString();
                                    year = reader["Year"].ToString();
                                    genre = reader["Genre"] != DBNull.Value ? reader["Genre"].ToString() : "N/A";
                                    rating = reader["IMDbRating"] != DBNull.Value ? reader["IMDbRating"].ToString() : "N/A";
                                }
                            }
                        }

                        // Add to favorites
                        string insertQuery = "INSERT INTO favourites (movie_id, user_id) VALUES (@movieId, @userId)";
                        using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@movieId", movieId);
                            insertCommand.Parameters.AddWithValue("@userId", currentUserId);
                            insertCommand.ExecuteNonQuery();
                        }

                        // Add to UI list
                        favorites.Add(new Movie
                        {
                            Title = title,
                            Year = year,
                            Genre = genre,
                            IMDbRating = rating
                        });

                        MessageBox.Show($"'{title}' added to favorites!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding movie to favorites: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddToWatchlist_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is int movieId)
            {
                try
                {
                    using (MySqlConnection conn = new MySqlConnection(ConnectionString))
                    {
                        conn.Open();

                        // Check if movie is already in watchlist
                        string checkQuery = "SELECT COUNT(*) FROM watchlist WHERE movie_id = @movieId AND user_id = @userId";
                        using (MySqlCommand checkCmd = new MySqlCommand(checkQuery, conn))
                        {
                            checkCmd.Parameters.AddWithValue("@movieId", movieId);
                            checkCmd.Parameters.AddWithValue("@userId", currentUserId);
                            long count = Convert.ToInt64(checkCmd.ExecuteScalar());

                            if (count > 0)
                            {
                                MessageBox.Show("This movie is already in your watchlist!", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                return;
                            }
                        }

                        // Get movie details for UI update
                        string detailsQuery = "SELECT Title, SUBSTRING(ReleaseDate, 1, 4) AS Year, Genre, IMDbRating FROM movies WHERE ID = @movieId";
                        string title = "";
                        string year = "N/A";
                        string genre = "N/A";
                        string rating = "N/A";

                        using (MySqlCommand detailsCommand = new MySqlCommand(detailsQuery, conn))
                        {
                            detailsCommand.Parameters.AddWithValue("@movieId", movieId);
                            using (MySqlDataReader reader = detailsCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    title = reader["Title"].ToString();
                                    year = reader["Year"].ToString();
                                    genre = reader["Genre"] != DBNull.Value ? reader["Genre"].ToString() : "N/A";
                                    rating = reader["IMDbRating"] != DBNull.Value ? reader["IMDbRating"].ToString() : "N/A";
                                }
                            }
                        }

                        // Add to watchlist
                        string insertQuery = "INSERT INTO watchlist (movie_id, user_id) VALUES (@movieId, @userId)";
                        using (MySqlCommand insertCmd = new MySqlCommand(insertQuery, conn))
                        {
                            insertCmd.Parameters.AddWithValue("@movieId", movieId);
                            insertCmd.Parameters.AddWithValue("@userId", currentUserId);
                            int rowsAffected = insertCmd.ExecuteNonQuery();

                            if (rowsAffected > 0)
                            {
                                // Add to UI list
                                watchlist.Add(new Movie
                                {
                                    Title = title,
                                    Year = year,
                                    Genre = genre,
                                    IMDbRating = rating
                                });
                                MessageBox.Show($"'{title}' added to watchlist!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                            }
                            else
                            {
                                MessageBox.Show($"Failed to add movie to watchlist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding to watchlist: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}