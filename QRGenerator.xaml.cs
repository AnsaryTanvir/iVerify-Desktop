using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ZXing;
using ZXing.Common;


namespace iVerify
{

    public partial class QRGenerator : Window
    {


        static string GenerateRandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            StringBuilder sb = new StringBuilder();

            Random random = new Random();
            for (int i = 0; i < length; i++)
            {
                int index = random.Next(chars.Length);
                sb.Append(chars[index]);
            }

            return sb.ToString();
        }


        public QRGenerator()
        {
            InitializeComponent();
            BatchID.Text = GenerateRandomString(8).ToUpper();
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var textBox = (TextBox)sender;
                var caretIndex = textBox.CaretIndex;

                // Insert a newline character at the current caret position
                textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);

                // Adjust the caret position to be after the inserted newline character
                textBox.CaretIndex = caretIndex + 1;

                // Mark the event as handled to prevent default behavior
                e.Handled = true;
            }
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


        private async void Generate_Button_Click(object sender, RoutedEventArgs e)
        {


            String genericID    = GenericID.Text;
            String name         = Name.Text;
            String category     = Category.Text;
            String manufacturer = Manufacturer.Text;
            String description  = Description.Text;
            
            String batchID  = BatchID.Text;
            String expiry   = Expiry.SelectedDate.Value.ToString("dd-MMM-yyyy");
            String quantity = Quantity.Text;

            //MessageBox.Show(genericID + " " + name + " " + category + " " + manufacturer + " " + description + " " + batchID + " " + expiry + " " + quantity);



            string exePath = Assembly.GetExecutingAssembly().Location;
            string exeDirectory = Path.GetDirectoryName(exePath);
            string nestedFolderPath = Path.Combine(exeDirectory, "QR Codes", category, manufacturer, genericID, batchID);
            if (!Directory.Exists(nestedFolderPath))
            {
                Directory.CreateDirectory(nestedFolderPath);
            }

            try
            {

                string basePath         = AppDomain.CurrentDomain.BaseDirectory;
                string publicKeyPath    = Path.Combine(basePath, "public_key.pem");
                string privateKeyPath   = Path.Combine(basePath, "private_key.pem");
                EncryptionHandler encryptionHandler = new EncryptionHandler();
                encryptionHandler.LoadKeys(publicKeyPath, privateKeyPath);
                

                ProgressBarQR.Minimum = 0;
                ProgressBarQR.Maximum = int.Parse(quantity);
                ProgressBarQR.Value = 0;
                ProgressBarQR.Visibility = Visibility.Visible;

                ProgressBarQRValue.Text = "0/" + quantity;
                ProgressBarQRValue.Visibility = Visibility.Visible;


                for (int i = 0; i < int.Parse(quantity); i++)
                {

                    Guid uuid = Guid.NewGuid();
                    byte[] encryptedData = encryptionHandler.EncryptData(uuid.ToString());
                    String encryptedUUID = BitConverter.ToString(encryptedData).Replace("-", "");

                    BarcodeWriter writer = new BarcodeWriter();
                    writer.Format = BarcodeFormat.QR_CODE;
                    EncodingOptions options = new EncodingOptions
                    {
                        Width = 150,
                        Height = 150,
                        Margin = 0
                    };
                    writer.Options = options;

                    Bitmap qrCodeBitmap = writer.Write(encryptedUUID);
                    string qrCodePath   = Path.Combine(nestedFolderPath, uuid.ToString() + ".png");

                    // Store the uuid in the database first the save the file
                    const string apiUrl = "https://iverify.gamingbd.xyz/api/add.php";


                    Dictionary<string, string> formData = new Dictionary<string, string>();
                    formData.Add("generic_id"   , genericID.ToString());
                    formData.Add("name"         , name.ToString());
                    formData.Add("category"     , category.ToString());
                    formData.Add("manufacturer" , manufacturer.ToString());
                    formData.Add("batch_id"     , batchID.ToString());
                    formData.Add("description"  , description.ToString());
                    formData.Add("expiry"       , expiry.ToString());
                    formData.Add("uuid"         , uuid.ToString());


                    string response = await NetworkRequest.PostHttpResponse(apiUrl, formData);
                    if (response.ToString().TrimEnd() == "Success".TrimEnd())
                    {
                        qrCodeBitmap.Save(qrCodePath);
                    }
                    else
                    {
                        MessageBox.Show("Failed to add the qr code in the server! Reason: " + response);
                    }

                    ProgressBarQR.Value = i+1;
                    ProgressBarQRValue.Text  = (i+1) + "/"+ quantity;


                    // Added delay for stability
                    await Task.Delay(10);

                }

                MessageBox.Show(quantity + " QR codes have been generated and stored in " + nestedFolderPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           

        }
    }
}
