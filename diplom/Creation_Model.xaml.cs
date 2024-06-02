using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace diplom
{
    /// <summary>
    /// Логика взаимодействия для Creation_Model.xaml
    /// </summary>
    public partial class Creation_Model : Window
    {
        public Creation_Model()
        {
            InitializeComponent();
        }
        private SqlConnection sqlConnection = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
            sqlConnection.Open();
            string query1 = "SELECT TOP 1 id FROM Models ORDER BY id DESC";

            using (SqlCommand command = new SqlCommand(query1, sqlConnection))
            {
                int lastId = (int)command.ExecuteScalar();
                id_model.Text = (lastId + 1).ToString();
            }
        }

        private void Button_image_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg, *.png, *.bmp)|*.jpg;*.png;*.bmp|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string imagePath = openFileDialog.FileName;
                BitmapImage bitmapImage = new BitmapImage(new Uri(imagePath));
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = new Image() { Source = bitmapImage };
                listBoxItem.Height = 60;
                listBoxItem.Width = 60;
                List_image.Items.Add(listBoxItem);
            }
        }

        private void Button_File_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "GCode files (*.gcode)|*.gcode|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                string gcodeFilePath = openFileDialog.FileName;
                ListBoxItem listBoxItem = new ListBoxItem();
                listBoxItem.Content = gcodeFilePath;
                List_File.Items.Add(listBoxItem);
            }



        }

        private void Button_create_Click(object sender, RoutedEventArgs e)
        {
            bool isAnyTextBoxEmpty = false;

            foreach (UIElement element in mainGrid.Children)
            {
                if (element is TextBox textBox)
                {
                    if (string.IsNullOrWhiteSpace(textBox.Text))
                    {
                        isAnyTextBoxEmpty = true;
                        break;
                    }
                }
            }
            if (isAnyTextBoxEmpty)
            {
                MessageBox.Show("Заполните все поля для создания модели!");
            }
            else
            {

                if (List_image.Items.Count > 0)
                {
                    ListBoxItem imageItem = (ListBoxItem)List_image.Items[0];
                    if (imageItem.Content is Image image)
                    {
                        byte[] imageBytes = ImageToBytes(image);

                        if (List_File.Items.Count > 0)
                        {
                            ListBoxItem gcodeItem = (ListBoxItem)List_File.Items[0];
                            if (gcodeItem.Content is string gcodeFilePath)
                            {
                                byte[] gcodeBytes = FileToBytes(gcodeFilePath);
                                sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
                                sqlConnection.Open();
                                using (SqlCommand cmd = new SqlCommand("INSERT INTO Models (Id,Image, File_model, Functional_Purpose, [Size_model(x)],[Size_model(y)],[Size_model(z)], Description,Name) VALUES (@ID, @Image, @File_model,@Function_model, @Size_modelX,@Size_modelY, @Size_modelZ, @Description_model, @Name)", sqlConnection))
                                {
                                    cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1).Value = imageBytes;
                                    cmd.Parameters.Add("@File_model", SqlDbType.VarBinary, -1).Value = gcodeBytes;
                                    cmd.Parameters.AddWithValue("@Name", Name_model.Text);
                                    cmd.Parameters.AddWithValue("@ID", id_model.Text);
                                    cmd.Parameters.AddWithValue("@Function_model", Function_model.Text);
                                    cmd.Parameters.AddWithValue("@Size_modelX", Size_modelX.Text);
                                    cmd.Parameters.AddWithValue("@Size_modelY", Size_modelY.Text);
                                    cmd.Parameters.AddWithValue("@Size_modelZ", Size_modelZ.Text);
                                    cmd.Parameters.AddWithValue("@Description_model", Description_model.Text);
                                    cmd.ExecuteNonQuery();
                                }
                                //window.AddGridToComboBox(id_model);
                                id_model.Text = (Convert.ToInt32(id_model.Text) + 1).ToString();
                                MessageBox.Show("Модель успешно создана!");
                            }

                        }

                    }

                }
            }
         
        }

        private byte[] ImageToBytes(Image image)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create((BitmapSource)image.Source));

            using (MemoryStream memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private byte[] FileToBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            this.IsEnabled = true;
        }
    }
}
