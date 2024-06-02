using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Логика взаимодействия для Redaction_Model.xaml
    /// </summary>
    public partial class Redaction_Model : Window
    {
        public string Id_project {  get; set; }
        private SqlConnection sqlConnection = null;
        public delegate void CreateProjectHandler();
        public event CreateProjectHandler CreateProjectEvent;
        public Redaction_Model()
        {
            InitializeComponent();
        }
        int id_models = 0;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
            sqlConnection.Open();
            
            string sqlQuery = "SELECT id_model FROM Projects WHERE id_project = @id";
            using (SqlCommand command = new SqlCommand(sqlQuery, sqlConnection))
            {
                command.Parameters.AddWithValue("@id", Convert.ToInt32(Id_project));
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    id_models = Convert.ToInt32(reader[0].ToString());
                }

                reader.Close();
            }

            string query = "SELECT *  FROM Models WHERE Id = @id ";

            using (SqlCommand command2 = new SqlCommand(query, sqlConnection))
            {
                command2.Parameters.AddWithValue("@id", id_models);
                SqlDataReader reader = command2.ExecuteReader();
                while (reader.Read())
                {
                    id_model.Text = reader[0].ToString();
                    Name_model.Text = reader[1].ToString();
                    Function_model.Text = reader[2].ToString();
                    Size_modelX.Text = reader[3].ToString();
                    Size_modelY.Text = reader[4].ToString();
                    Size_modelZ.Text = reader[5].ToString();
                    Description_model.Text = reader[8].ToString();
                    byte[] imageData = (byte[])reader[6];
                    if (imageData != null && imageData.Length > 0)
                    {
                        BitmapImage imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = new System.IO.MemoryStream(imageData);
                        imageSource.EndInit();
                        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                        image.Source = imageSource;
                        image.Width = 60;
                        image.Height = 60;
                        ListBoxItem listBoxItem = new ListBoxItem();
                        listBoxItem.Content = image;
                        List_image.Items.Add(listBoxItem);
                    }
                   


                }
                reader.Close();
            }

        }

        private void image_delete_Click(object sender, RoutedEventArgs e)
        {
            List_image.Items.Clear();
        }

        private void file_delete_Click(object sender, RoutedEventArgs e)
        {
            List_File.Items.Clear();
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
                listBoxItem.Content = new System.Windows.Controls.Image() { Source = bitmapImage };
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
        private void Button_redaction_Click(object sender, RoutedEventArgs e)
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
                                using (SqlCommand cmd = new SqlCommand("UPDATE Models SET Image = @Image, File_model = @File_model, Functional_Purpose = @Function_model, [Size_model(x)] = @Size_modelX, [Size_model(y)] = @Size_modelY, [Size_model(z)] = @Size_modelZ, Description = @Description_model, Name = @Name WHERE Id = @id_models", sqlConnection))
                                {
                                    cmd.Parameters.AddWithValue("@id_models", Convert.ToInt32(id_models)); 
                                    cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1).Value = imageBytes;
                                    cmd.Parameters.Add("@File_model", SqlDbType.VarBinary, -1).Value = gcodeBytes;
                                    cmd.Parameters.AddWithValue("@Name", Name_model.Text);
                                    cmd.Parameters.AddWithValue("@Function_model", Function_model.Text); 
                                    cmd.Parameters.AddWithValue("@Size_modelX", Convert.ToDecimal(Size_modelX.Text));
                                    cmd.Parameters.AddWithValue("@Size_modelY", Convert.ToDecimal(Size_modelY.Text));
                                    cmd.Parameters.AddWithValue("@Size_modelZ", Convert.ToDecimal(Size_modelZ.Text));
                                    cmd.Parameters.AddWithValue("@Description_model", Description_model.Text);
                                    cmd.ExecuteNonQuery();
                                }
                                //window.AddGridToComboBox(id_model);
                                MessageBox.Show("Модель успешно обновлена!");
                            }
                            CreateProjectEvent();
                            this.Close();
                        }
                        else
                        {
                            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
                            sqlConnection.Open();
                            using (SqlCommand cmd = new SqlCommand("UPDATE Models SET Image = @Image, Functional_Purpose = @Function_model, [Size_model(x)] = @Size_modelX, [Size_model(y)] = @Size_modelY, [Size_model(z)] = @Size_modelZ, Description = @Description_model, Name = @Name WHERE Id = @id_models", sqlConnection))
                            {
                                cmd.Parameters.AddWithValue("@id_models", Convert.ToInt32(id_models)); 
                                cmd.Parameters.Add("@Image", SqlDbType.VarBinary, -1).Value = imageBytes;
                                cmd.Parameters.AddWithValue("@Name", Name_model.Text);
                                cmd.Parameters.AddWithValue("@Function_model", Function_model.Text); 
                                cmd.Parameters.AddWithValue("@Size_modelX", Convert.ToDecimal(Size_modelX.Text));
                                cmd.Parameters.AddWithValue("@Size_modelY", Convert.ToDecimal(Size_modelY.Text));
                                cmd.Parameters.AddWithValue("@Size_modelZ", Convert.ToDecimal(Size_modelZ.Text));
                                cmd.Parameters.AddWithValue("@Description_model", Description_model.Text);
                                cmd.ExecuteNonQuery();
                            }
                            MessageBox.Show("Модель успешно обновлена!");
                            CreateProjectEvent();
                            this.Close();
                        }
                        
                    }
                }
            }

        }
    }
}
