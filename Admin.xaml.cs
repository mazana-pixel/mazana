using System;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MySql.Data.MySqlClient;

namespace mazanabe
{
    public partial class AdminDashboard : Window
    {
        private readonly string connectionString = "Server=127.0.0.1;Uid=root;Database=mazana;Password=new_password;AllowPublicKeyRetrieval=true;SslMode=none;";
        public AdminDashboard()
        {
            InitializeComponent();
            txtStatus.Text = "Application started";
            LoadUsers(); // Load users when the window is initialized
            LoadMovies();
        }


        private void BtnTestConnection_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    txtConnectionStatus.Text = "✓ Connection successful!";
                    txtConnectionStatus.Foreground = Brushes.Green;
                    txtStatus.Text = "Connected to database";
                }
            }
            catch (Exception ex)
            {
                txtConnectionStatus.Text = "✗ Connection failed";
                txtConnectionStatus.Foreground = Brushes.Red;
                txtStatus.Text = $"Error: {ex.Message}";
                MessageBox.Show($"Connection failed:\n{ex.Message}", "Error",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAddUser_Click(object sender, RoutedEventArgs e)
        {
            // Retrieve inputs
            var username = txtUsername.Text.Trim();
            var email = txtEmail.Text.Trim();
            var password = txtPassword.Password.Trim();
            var userType = userTypeComboBox.SelectedItem;

            // Check if any field is empty
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password) ||
                userType == null)
            {
                MessageBox.Show("Please fill all fields", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate email format
            if (!IsValidEmail(email))
            {
                MessageBox.Show("Please enter a valid email address", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Validate password strength
            if (!IsStrongPassword(password))
            {
                MessageBox.Show("Password must be at least 8 characters long and include an uppercase letter, " +
                                "a lowercase letter, a number, and a special character.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Add user to the database
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    var query = "INSERT INTO Users (Username, Email, Password, UserType) VALUES (@user, @email, @pwd, @type)";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@user", username);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@pwd", password); // Hash in production
                        cmd.Parameters.AddWithValue("@type", userType.ToString());

                        cmd.ExecuteNonQuery();
                        txtStatus.Text = "User added successfully";
                        MessageBox.Show("User added!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        LoadUsers();
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error adding user: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private bool IsValidEmail(string email)
        {
            try
            {
                var emailChecker = new System.Net.Mail.MailAddress(email);
                return emailChecker.Address == email;
            }
            catch
            {
                return false;
            }
        }
        private bool IsStrongPassword(string password)
        {
            // Ensure password is at least 8 characters long
            if (password.Length < 8) return false;

            // Check for at least one uppercase letter, one lowercase letter, one digit, and one special character
            bool hasUpperCase = password.Any(char.IsUpper);
            bool hasLowerCase = password.Any(char.IsLower);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecialChar = password.Any(ch => !char.IsLetterOrDigit(ch));

            return hasUpperCase && hasLowerCase && hasDigit && hasSpecialChar;
        }


        private void LoadUsers()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                using (var adapter = new MySqlDataAdapter("SELECT ID, Username, Email, UserType FROM Users", connection))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    usersDataGrid.ItemsSource = dt.DefaultView;
                    userCountTextBlock.Text = $"Total Users: {dt.Rows.Count}";
                    txtStatus.Text = $"Loaded {dt.Rows.Count} users";
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading users: {ex.Message}";
            }
        }
        private void BtnRemoveUser_Click(object sender, RoutedEventArgs e)
        {
            // Check if a user is selected
            if (usersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to remove.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Retrieve the selected user's email
                var selectedRow = (DataRowView)usersDataGrid.SelectedItem;
                var userEmail = selectedRow["Email"].ToString(); // 'Email' column in the database

                // Confirm deletion
                var result = MessageBox.Show($"Are you sure you want to delete the user with Email: {userEmail}?",
                                             "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        // SQL command to delete the user
                        var query = "DELETE FROM Users WHERE Email = @userEmail";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@userEmail", userEmail); // Pass the selected user's email
                            cmd.ExecuteNonQuery(); // Execute the deletion command
                        }

                        txtStatus.Text = $"User with Email {userEmail} removed successfully.";
                        MessageBox.Show($"User with Email {userEmail} has been removed.", "Success",
                                        MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the DataGrid to reflect changes
                        LoadUsers();
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error removing user: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditUser_Click(object sender, RoutedEventArgs e)
        {
            if (usersDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a user to edit.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Retrieve selected user data
                var selectedRow = (DataRowView)usersDataGrid.SelectedItem;
                var username = selectedRow["Username"].ToString();
                var email = selectedRow["Email"].ToString();
                var userType = selectedRow["UserType"].ToString();

                // Open EditUserDialog
                var editDialog = new EditUserDialog(username, email, userType);
                if (editDialog.ShowDialog() == true)
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        // Update user in database
                        var query = "UPDATE Users SET Username = @username, UserType = @userType WHERE Email = @userEmail";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@username", editDialog.Username);
                            cmd.Parameters.AddWithValue("@userType", editDialog.UserType);
                            cmd.Parameters.AddWithValue("@userEmail", editDialog.Email);
                            cmd.ExecuteNonQuery();
                        }

                        txtStatus.Text = $"User {editDialog.Username} updated successfully.";
                        MessageBox.Show("User updated successfully.", "Success",
                                        MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the DataGrid
                        LoadUsers();
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error updating user: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void BtnAddMovie_Click(object sender, RoutedEventArgs e)
        {
            // Gather movie details from the UI
            string movieTitle = txtMovieTitle.Text;
            string director = txtDirector.Text;
            int releaseDate;
            string genre = txtGenre.Text;
            decimal imdbRating;
            decimal boxOfficeGross;
            decimal budget;
            int duration;
            string plotSummary = txtPlotSummary.Text;
            string mainCharacter = txtMainCharacter.Text; // Main character's name
            string productionCompany = txtProductionCompany.Text;
            string language = txtLanguage.Text;
            string country = txtCountry.Text;
            string awards = txtAwards.Text;
            string streamingPlatform = txtStreamingPlatform.Text;

            // Ensure that ComboBox selections are valid
            string movieType = (cmbMovieType.SelectedItem as ComboBoxItem)?.Content.ToString();
            string ageGroup = (cmbAgeGroup.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Check if required fields are filled
            if (string.IsNullOrWhiteSpace(movieTitle) || string.IsNullOrWhiteSpace(director) || string.IsNullOrWhiteSpace(mainCharacter))
            {
                MessageBox.Show("Please fill in all required fields.", "Missing Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Convert string to the correct data types and handle conversion errors
            if (!int.TryParse(txtReleaseDate.Text, out releaseDate) ||
                !decimal.TryParse(txtIMDbRating.Text, out imdbRating) ||
                !decimal.TryParse(txtBoxOfficeGross.Text, out boxOfficeGross) ||
                !decimal.TryParse(txtBudget.Text, out budget) ||
                !int.TryParse(txtDuration.Text, out duration))
            {
                MessageBox.Show("Please enter valid numeric values for year, ratings, and financials.", "Invalid Data", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Check if ComboBox selections are valid
            if (string.IsNullOrEmpty(movieType) || string.IsNullOrEmpty(ageGroup))
            {
                MessageBox.Show("Please select a valid movie type and age group.", "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Insert movie into the database
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"INSERT INTO Movies (Title, Director, ReleaseDate, Genre, IMDbRating, BoxOfficeGross, Budget, Duration, PlotSummary, MainCharacter, ProductionCompany, Language, Country, Awards, StreamingPlatform, MovieType, AgeGroup)
VALUES (@movieTitle, @director, @releaseDate, @genre, @imdbRating, @boxOfficeGross, @budget, @duration, @plotSummary, @mainCharacter, @productionCompany, @language, @country, @awards, @streamingPlatform, @movieType, @ageGroup);";

                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@movieTitle", movieTitle);
                        cmd.Parameters.AddWithValue("@director", director);
                        cmd.Parameters.AddWithValue("@releaseDate", releaseDate);
                        cmd.Parameters.AddWithValue("@genre", genre);
                        cmd.Parameters.AddWithValue("@imdbRating", imdbRating);
                        cmd.Parameters.AddWithValue("@boxOfficeGross", boxOfficeGross);
                        cmd.Parameters.AddWithValue("@budget", budget);
                        cmd.Parameters.AddWithValue("@duration", duration);
                        cmd.Parameters.AddWithValue("@plotSummary", plotSummary);
                        cmd.Parameters.AddWithValue("@mainCharacter", mainCharacter);
                        cmd.Parameters.AddWithValue("@productionCompany", productionCompany);
                        cmd.Parameters.AddWithValue("@language", language);
                        cmd.Parameters.AddWithValue("@country", country);
                        cmd.Parameters.AddWithValue("@awards", awards);
                        cmd.Parameters.AddWithValue("@streamingPlatform", streamingPlatform);
                        cmd.Parameters.AddWithValue("@movieType", movieType);
                        cmd.Parameters.AddWithValue("@ageGroup", ageGroup);

                        cmd.ExecuteNonQuery(); // Execute the query to insert the movie
                    }

                    MessageBox.Show("Movie added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding movie: {ex.Message}", "Database Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void txtMovieTitle_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            if (txtBox.Text == "Enter Movie Title")
            {
                txtBox.Text = "";
            }
        }

        private void txtMovieTitle_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(txtBox.Text))
            {
                txtBox.Text = "Enter Movie Title";
            }
        }
        private void LoadMovies()
        {
            try
            {
                using (var connection = new MySqlConnection(connectionString))
                using (var adapter = new MySqlDataAdapter("SELECT ID, Title, Director, ReleaseDate as ReleaseYear, Genre, IMDbRating, Duration FROM Movies", connection))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    moviesDataGrid.ItemsSource = dt.DefaultView;
                    movieCountTextBlock.Text = $"Total Movies: {dt.Rows.Count}";
                    txtStatus.Text = $"Loaded {dt.Rows.Count} movies";
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error loading movies: {ex.Message}";
            }
        }

        private void BtnRemoveMovie_Click(object sender, RoutedEventArgs e)
        {
            // Check if a movie is selected
            if (moviesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a movie to remove.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Retrieve the selected movie's title
                var selectedRow = (DataRowView)moviesDataGrid.SelectedItem;
                var movieId = selectedRow["ID"].ToString();
                var movieTitle = selectedRow["Title"].ToString();

                // Confirm deletion
                var result = MessageBox.Show($"Are you sure you want to delete the movie: {movieTitle}?",
                                             "Confirm Deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    using (var connection = new MySqlConnection(connectionString))
                    {
                        connection.Open();

                        // SQL command to delete the movie
                        var query = "DELETE FROM Movies WHERE ID = @movieId";
                        using (var cmd = new MySqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@movieId", movieId);
                            cmd.ExecuteNonQuery(); // Execute the deletion command
                        }

                        txtStatus.Text = $"Movie '{movieTitle}' removed successfully.";
                        MessageBox.Show($"Movie '{movieTitle}' has been removed.", "Success",
                                        MessageBoxButton.OK, MessageBoxImage.Information);

                        // Refresh the DataGrid to reflect changes
                        LoadMovies();
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error removing movie: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnEditMovie_Click(object sender, RoutedEventArgs e)
        {
            if (moviesDataGrid.SelectedItem == null)
            {
                MessageBox.Show("Please select a movie to edit.", "No Selection",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Retrieve selected movie data
                var selectedRow = (DataRowView)moviesDataGrid.SelectedItem;
                int movieId = Convert.ToInt32(selectedRow["ID"]);

                // Fetch complete movie details from the database
                using (var connection = new MySqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Movies WHERE ID = @movieId";
                    using (var cmd = new MySqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@movieId", movieId);
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Get all movie details from the database
                                var title = reader["Title"].ToString();
                                var director = reader["Director"].ToString();
                                int releaseDate = Convert.ToInt32(reader["ReleaseDate"]);
                                var genre = reader["Genre"].ToString();
                                decimal imdbRating = reader["IMDbRating"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["IMDbRating"]);
                                decimal boxOfficeGross = reader["BoxOfficeGross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["BoxOfficeGross"]);
                                decimal budget = reader["Budget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Budget"]);
                                int duration = reader["Duration"] == DBNull.Value ? 0 : Convert.ToInt32(reader["Duration"]);
                                var plotSummary = reader["PlotSummary"] == DBNull.Value ? "" : reader["PlotSummary"].ToString();
                                var mainCharacter = reader["MainCharacter"] == DBNull.Value ? "" : reader["MainCharacter"].ToString();
                                var productionCompany = reader["ProductionCompany"] == DBNull.Value ? "" : reader["ProductionCompany"].ToString();
                                var language = reader["Language"] == DBNull.Value ? "" : reader["Language"].ToString();
                                var country = reader["Country"] == DBNull.Value ? "" : reader["Country"].ToString();
                                var awards = reader["Awards"] == DBNull.Value ? "" : reader["Awards"].ToString();
                                var streamingPlatform = reader["StreamingPlatform"] == DBNull.Value ? "" : reader["StreamingPlatform"].ToString();
                                var movieType = reader["MovieType"] == DBNull.Value ? "" : reader["MovieType"].ToString();
                                var ageGroup = reader["AgeGroup"] == DBNull.Value ? "" : reader["AgeGroup"].ToString();

                                // Open EditMovieDialog
                                var editDialog = new EditMovieDialog(movieId, title, director, releaseDate, genre,
                                    imdbRating, boxOfficeGross, budget, duration, plotSummary, mainCharacter,
                                    productionCompany, language, country, awards, streamingPlatform, movieType, ageGroup);

                                if (editDialog.ShowDialog() == true)
                                {
                                    // Update movie in database with the values returned from the dialog
                                    UpdateMovieInDatabase(editDialog);

                                    txtStatus.Text = $"Movie '{editDialog.Title}' updated successfully.";
                                    MessageBox.Show("Movie updated successfully.", "Success",
                                                    MessageBoxButton.OK, MessageBoxImage.Information);

                                    // Refresh the DataGrid
                                    LoadMovies();
                                }
                            }
                            else
                            {
                                MessageBox.Show("Movie not found in the database.", "Error",
                                                MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                txtStatus.Text = $"Error retrieving movie details: {ex.Message}";
                MessageBox.Show($"Error: {ex.Message}", "Database Error",
                                MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UpdateMovieInDatabase(EditMovieDialog movieData)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                // Update movie in database
                var query = @"UPDATE Movies SET 
                    Title = @title, 
                    Director = @director, 
                    ReleaseDate = @releaseDate, 
                    Genre = @genre, 
                    IMDbRating = @imdbRating, 
                    BoxOfficeGross = @boxOfficeGross, 
                    Budget = @budget, 
                    Duration = @duration, 
                    PlotSummary = @plotSummary, 
                    MainCharacter = @mainCharacter, 
                    ProductionCompany = @productionCompany, 
                    Language = @language, 
                    Country = @country, 
                    Awards = @awards, 
                    StreamingPlatform = @streamingPlatform, 
                    MovieType = @movieType, 
                    AgeGroup = @ageGroup
                    WHERE ID = @movieId";

                using (var cmd = new MySqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@movieId", movieData.MovieId);
                    cmd.Parameters.AddWithValue("@title", movieData.Title);
                    cmd.Parameters.AddWithValue("@director", movieData.Director);
                    cmd.Parameters.AddWithValue("@releaseDate", movieData.ReleaseDate);
                    cmd.Parameters.AddWithValue("@genre", movieData.Genre);
                    cmd.Parameters.AddWithValue("@imdbRating", movieData.IMDbRating);
                    cmd.Parameters.AddWithValue("@boxOfficeGross", movieData.BoxOfficeGross);
                    cmd.Parameters.AddWithValue("@budget", movieData.Budget);
                    cmd.Parameters.AddWithValue("@duration", movieData.Duration);
                    cmd.Parameters.AddWithValue("@plotSummary", movieData.PlotSummary);
                    cmd.Parameters.AddWithValue("@mainCharacter", movieData.MainCharacter);
                    cmd.Parameters.AddWithValue("@productionCompany", movieData.ProductionCompany);
                    cmd.Parameters.AddWithValue("@language", movieData.Language);
                    cmd.Parameters.AddWithValue("@country", movieData.Country);
                    cmd.Parameters.AddWithValue("@awards", movieData.Awards);
                    cmd.Parameters.AddWithValue("@streamingPlatform", movieData.StreamingPlatform);
                    cmd.Parameters.AddWithValue("@movieType", movieData.MovieType);
                    cmd.Parameters.AddWithValue("@ageGroup", movieData.AgeGroup);

                    cmd.ExecuteNonQuery();
                }
            }
        }
        public class EditMovieDialog : Window
        {
            public int MovieId { get; private set; }
            public string Title { get; private set; }
            public string Director { get; private set; }
            public int ReleaseDate { get; private set; }
            public string Genre { get; private set; }
            public decimal IMDbRating { get; private set; }
            public decimal BoxOfficeGross { get; private set; }
            public decimal Budget { get; private set; }
            public int Duration { get; private set; }
            public string PlotSummary { get; private set; }
            public string MainCharacter { get; private set; }
            public string ProductionCompany { get; private set; }
            public string Language { get; private set; }
            public string Country { get; private set; }
            public string Awards { get; private set; }
            public string StreamingPlatform { get; private set; }
            public string MovieType { get; private set; }
            public string AgeGroup { get; private set; }

            private TextBox txtTitle;
            private TextBox txtDirector;
            private TextBox txtReleaseDate;
            private TextBox txtGenre;
            private TextBox txtIMDbRating;
            private TextBox txtBoxOfficeGross;
            private TextBox txtBudget;
            private TextBox txtDuration;
            private TextBox txtPlotSummary;
            private TextBox txtMainCharacter;
            private TextBox txtProductionCompany;
            private TextBox txtLanguage;
            private TextBox txtCountry;
            private TextBox txtAwards;
            private TextBox txtStreamingPlatform;
            private ComboBox cmbMovieType;
            private ComboBox cmbAgeGroup;

            public EditMovieDialog(int movieId, string title, string director, int releaseDate, string genre,
                                 decimal imdbRating, decimal boxOfficeGross, decimal budget, int duration,
                                 string plotSummary, string mainCharacter, string productionCompany,
                                 string language, string country, string awards, string streamingPlatform,
                                 string movieType, string ageGroup)
            {
                MovieId = movieId;

                // Set window properties
                Title = "Edit Movie";
                Width = 600;
                Height = 650;
                WindowStartupLocation = WindowStartupLocation.CenterOwner;

                // Create the main grid
                var grid = new Grid();
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

                // Header
                var header = new TextBlock
                {
                    Text = "Edit Movie",
                    FontSize = 18,
                    FontWeight = FontWeights.Bold,
                    Margin = new Thickness(10)
                };
                grid.Children.Add(header);

                // Create a TabControl similar to the main window
                var tabControl = new TabControl();
                Grid.SetRow(tabControl, 1);

                // Create the basic info tab
                var basicInfoTab = new TabItem { Header = "Basic Information" };
                var basicInfoGrid = new Grid { Margin = new Thickness(10) };

                basicInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                basicInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                for (int i = 0; i < 9; i++)
                {
                    basicInfoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                // Add fields to basic info tab
                AddField(basicInfoGrid, 0, "Title:", out txtTitle, title);
                AddField(basicInfoGrid, 1, "Director:", out txtDirector, director);
                AddField(basicInfoGrid, 2, "Release Date:", out txtReleaseDate, releaseDate.ToString());
                AddField(basicInfoGrid, 3, "Genre:", out txtGenre, genre);
                AddField(basicInfoGrid, 4, "Language:", out txtLanguage, language);
                AddField(basicInfoGrid, 5, "Country:", out txtCountry, country);
                AddField(basicInfoGrid, 6, "Duration (Minutes):", out txtDuration, duration.ToString());
                AddField(basicInfoGrid, 7, "Main Character:", out txtMainCharacter, mainCharacter);

                // Plot summary needs custom handling for height
                basicInfoGrid.Children.Add(new TextBlock
                {
                    Text = "Plot Summary:",
                    Margin = new Thickness(5),
                    VerticalAlignment = VerticalAlignment.Top
                });
                Grid.SetRow(basicInfoGrid.Children[basicInfoGrid.Children.Count - 1], 8);
                Grid.SetColumn(basicInfoGrid.Children[basicInfoGrid.Children.Count - 1], 0);

                txtPlotSummary = new TextBox
                {
                    Text = plotSummary,
                    Margin = new Thickness(5),
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true,
                    Height = 80
                };
                basicInfoGrid.Children.Add(txtPlotSummary);
                Grid.SetRow(txtPlotSummary, 8);
                Grid.SetColumn(txtPlotSummary, 1);

                var basicInfoScroll = new ScrollViewer { Content = basicInfoGrid };
                basicInfoTab.Content = basicInfoScroll;

                // Create the additional info tab
                var additionalInfoTab = new TabItem { Header = "Additional Information" };
                var additionalInfoGrid = new Grid { Margin = new Thickness(10) };

                additionalInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                additionalInfoGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                for (int i = 0; i < 8; i++)
                {
                    additionalInfoGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }

                // Add fields to additional info tab
                AddField(additionalInfoGrid, 0, "IMDb Rating:", out txtIMDbRating, imdbRating.ToString());
                AddField(additionalInfoGrid, 1, "Box Office Gross:", out txtBoxOfficeGross, boxOfficeGross.ToString());
                AddField(additionalInfoGrid, 2, "Budget:", out txtBudget, budget.ToString());
                AddField(additionalInfoGrid, 3, "Production Company:", out txtProductionCompany, productionCompany);
                AddField(additionalInfoGrid, 4, "Awards:", out txtAwards, awards);
                AddField(additionalInfoGrid, 5, "Streaming Platform:", out txtStreamingPlatform, streamingPlatform);

                // Add ComboBox for Movie Type
                additionalInfoGrid.Children.Add(new TextBlock { Text = "Movie Type:", Margin = new Thickness(5) });
                Grid.SetRow(additionalInfoGrid.Children[additionalInfoGrid.Children.Count - 1], 6);
                Grid.SetColumn(additionalInfoGrid.Children[additionalInfoGrid.Children.Count - 1], 0);

                cmbMovieType = new ComboBox { Margin = new Thickness(5) };
                cmbMovieType.Items.Add(new ComboBoxItem { Content = "Action" });
                cmbMovieType.Items.Add(new ComboBoxItem { Content = "Drama" });
                cmbMovieType.Items.Add(new ComboBoxItem { Content = "Comedy" });
                cmbMovieType.Items.Add(new ComboBoxItem { Content = "Thriller" });
                cmbMovieType.Items.Add(new ComboBoxItem { Content = "Horror" });
                cmbMovieType.Items.Add(new ComboBoxItem { Content = "Sci-Fi" });

                // Select the current movie type
                foreach (ComboBoxItem item in cmbMovieType.Items)
                {
                    if (item.Content.ToString() == movieType)
                    {
                        cmbMovieType.SelectedItem = item;
                        break;
                    }
                }

                additionalInfoGrid.Children.Add(cmbMovieType);
                Grid.SetRow(cmbMovieType, 6);
                Grid.SetColumn(cmbMovieType, 1);

                // Add ComboBox for Age Group
                additionalInfoGrid.Children.Add(new TextBlock { Text = "Age Group:", Margin = new Thickness(5) });
                Grid.SetRow(additionalInfoGrid.Children[additionalInfoGrid.Children.Count - 1], 7);
                Grid.SetColumn(additionalInfoGrid.Children[additionalInfoGrid.Children.Count - 1], 0);

                cmbAgeGroup = new ComboBox { Margin = new Thickness(5) };
                cmbAgeGroup.Items.Add(new ComboBoxItem { Content = "All Ages" });
                cmbAgeGroup.Items.Add(new ComboBoxItem { Content = "Kids" });
                cmbAgeGroup.Items.Add(new ComboBoxItem { Content = "Teens" });
                cmbAgeGroup.Items.Add(new ComboBoxItem { Content = "Adults" });

                // Select the current age group
                foreach (ComboBoxItem item in cmbAgeGroup.Items)
                {
                    if (item.Content.ToString() == ageGroup)
                    {
                        cmbAgeGroup.SelectedItem = item;
                        break;
                    }
                }

                additionalInfoGrid.Children.Add(cmbAgeGroup);
                Grid.SetRow(cmbAgeGroup, 7);
                Grid.SetColumn(cmbAgeGroup, 1);

                var additionalInfoScroll = new ScrollViewer { Content = additionalInfoGrid };
                additionalInfoTab.Content = additionalInfoScroll;

                // Add tabs to TabControl
                tabControl.Items.Add(basicInfoTab);
                tabControl.Items.Add(additionalInfoTab);

                grid.Children.Add(tabControl);

                // Create buttons panel
                var buttonsPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(10)
                };

                var saveButton = new Button
                {
                    Content = "Save",
                    Width = 80,
                    Height = 30,
                    Margin = new Thickness(0, 0, 10, 0),
                    IsDefault = true
                };
                saveButton.Click += SaveButton_Click;
                buttonsPanel.Children.Add(saveButton);

                var cancelButton = new Button
                {
                    Content = "Cancel",
                    Width = 80,
                    Height = 30,
                    IsCancel = true
                };
                buttonsPanel.Children.Add(cancelButton);

                Grid.SetRow(buttonsPanel, 2);
                grid.Children.Add(buttonsPanel);

                Content = grid;
            }

            private void AddField(Grid grid, int row, string label, out TextBox textBox, string value)
            {
                grid.Children.Add(new TextBlock { Text = label, Margin = new Thickness(5) });
                Grid.SetRow(grid.Children[grid.Children.Count - 1], row);
                Grid.SetColumn(grid.Children[grid.Children.Count - 1], 0);

                textBox = new TextBox { Text = value, Margin = new Thickness(5) };
                grid.Children.Add(textBox);
                Grid.SetRow(textBox, row);
                Grid.SetColumn(textBox, 1);
            }

            private void SaveButton_Click(object sender, RoutedEventArgs e)
            {
                // Validate required fields
                if (string.IsNullOrWhiteSpace(txtTitle.Text) ||
                    string.IsNullOrWhiteSpace(txtDirector.Text) ||
                    string.IsNullOrWhiteSpace(txtMainCharacter.Text))
                {
                    MessageBox.Show("Please fill in all required fields (Title, Director, Main Character).",
                                    "Missing Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Validate numeric fields
                if (!int.TryParse(txtReleaseDate.Text, out int releaseDate) ||
                    !decimal.TryParse(txtIMDbRating.Text, out decimal imdbRating) ||
                    !decimal.TryParse(txtBoxOfficeGross.Text, out decimal boxOfficeGross) ||
                    !decimal.TryParse(txtBudget.Text, out decimal budget) ||
                    !int.TryParse(txtDuration.Text, out int duration))
                {
                    MessageBox.Show("Please enter valid numeric values for Release Date, IMDb Rating, Box Office, Budget, and Duration.",
                                   "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Check if ComboBox selections are valid
                if (cmbMovieType.SelectedItem == null || cmbAgeGroup.SelectedItem == null)
                {
                    MessageBox.Show("Please select a Movie Type and Age Group.",
                                  "Missing Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Store updated values
                Title = txtTitle.Text;
                Director = txtDirector.Text;
                ReleaseDate = releaseDate;
                Genre = txtGenre.Text;
                IMDbRating = imdbRating;
                BoxOfficeGross = boxOfficeGross;
                Budget = budget;
                Duration = duration;
                PlotSummary = txtPlotSummary.Text;
                MainCharacter = txtMainCharacter.Text;
                ProductionCompany = txtProductionCompany.Text;
                Language = txtLanguage.Text;
                Country = txtCountry.Text;
                Awards = txtAwards.Text;
                StreamingPlatform = txtStreamingPlatform.Text;
                MovieType = (cmbMovieType.SelectedItem as ComboBoxItem)?.Content.ToString();
                AgeGroup = (cmbAgeGroup.SelectedItem as ComboBoxItem)?.Content.ToString();

                DialogResult = true;
            }
        }

    }
}