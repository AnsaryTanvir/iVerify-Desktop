using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;

namespace iVerify
{

    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
      
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            // Start dragging the window using the built-in DragMove method
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }


        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {


            string username = UsernameTextBox.Text;
            string password = PasswordTextBox.Password;

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Please enter both username and password.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!NetworkInterface.GetIsNetworkAvailable())
            {
                MessageBox.Show("Sorry! Internet is not available.");
            }

            try
            {

                const string apiUrl  = "https://iverify.gamingbd.xyz/api/login.php";

                Dictionary<string, string> formData = new Dictionary<string, string>();
                formData.Add("username", username);
                formData.Add("password", password);
               
                string response = await NetworkRequest.PostHttpResponse(apiUrl, formData);
                if ( response.ToString().TrimEnd() == "Success".TrimEnd())
                {
                    QRGenerator qrGenerator = new QRGenerator();
                    qrGenerator.Show();
                    this.Close();
                }
                else
                {
                    MessageBox.Show(response);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
