using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace mazanabe
{
    public partial class EditUserDialog : Window
    {
        public string Username { get; private set; }
        public string Email { get; private set; }
        public string UserType { get; private set; }

        public EditUserDialog(string username, string email, string userType)
        {
            InitializeComponent();

            // Populate fields with current data
            txtUsername.Text = username;
            txtEmail.Text = email;
            cmbUserType.SelectedItem = cmbUserType.Items.Cast<ComboBoxItem>()
                .FirstOrDefault(item => item.Content.ToString() == userType);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                cmbUserType.SelectedItem == null)
            {
                MessageBox.Show("All fields must be filled.", "Validation Error",
                                MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Assign properties
            Username = txtUsername.Text;
            Email = txtEmail.Text;
            UserType = ((ComboBoxItem)cmbUserType.SelectedItem).Content.ToString();

            // Close dialog with success result
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Close dialog without saving
            DialogResult = false;
        }
    }
}
