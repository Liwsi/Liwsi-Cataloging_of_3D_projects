using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
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
    /// Логика взаимодействия для Creation_Window.xaml
    /// </summary>
    public partial class Creation_Window : Window
    {
        private MainWindow mainWindow;
        private Grid createdGrid;
        public delegate void CreateProjectHandler();
        public event CreateProjectHandler CreateProjectEvent;
        public Creation_Window(MainWindow main)
        {
            InitializeComponent();
            mainWindow = main;
        }
        string model_iD = "";
        string model_Name = "";
        string id_projects = "";

        public void AddValue(string id, string name)
        {
            model_iD = id; model_Name = name;
            id_list.Text = name;
        }
        private SqlConnection sqlConnection = null;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
            sqlConnection.Open();
            string query1 = "SELECT TOP 1 id_project FROM Projects ORDER BY id_project DESC";

            using (SqlCommand command1 = new SqlCommand(query1, sqlConnection))
            {
                int lastId = (int)command1.ExecuteScalar();
                id_project.Text = (lastId + 1).ToString();
            }

            string query3 = "SELECT Name  FROM Printers";

            using (SqlCommand command3 = new SqlCommand(query3, sqlConnection))
            {
                SqlDataReader reader = command3.ExecuteReader();

                while (reader.Read())
                {
                    id_printers.Items.Add(reader[0]);
                }
                reader.Close();
            }
            string query4 = "SELECT Name  FROM Plastics";

            using (SqlCommand command4 = new SqlCommand(query4, sqlConnection))
            {
                SqlDataReader reader = command4.ExecuteReader();

                while (reader.Read())
                {
                    id_plastic.Items.Add(reader[0]);
                }
                reader.Close();
            }

        }

        private void Button_CreateProject_Click(object sender, RoutedEventArgs e)
        {
            bool isAnyTextBoxEmpty = false;

            foreach (UIElement element in Grid_main.Children)
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
            if (isAnyTextBoxEmpty || Date.SelectedDate == null)
            {
                MessageBox.Show("Заполните все поля для создания проекта!");
            }
            else
            {
                string query = "INSERT INTO Projects (Id_project, id_printer, id_plastic, id_model, id_settings, Name_project, Author, Date, Comment, Result) " +
                  "VALUES (@IdProject, @IdPrinter, @IdPlastic, @IdModel, @IdSettings, @NameProject, @Author, @Date, @Comment, @Result)";
                int selectedId;
                string Plastic = "";
                int id_print = id_printers.SelectedIndex + 1;
                int id_pl = id_plastic.SelectedIndex + 1;
                DateTime selectedDate = Date.SelectedDate ?? DateTime.MinValue;
                using (SqlCommand command = new SqlCommand(query, sqlConnection))
                {
                    command.Parameters.AddWithValue("@IdProject", id_project.Text);
                    command.Parameters.AddWithValue("@IdPrinter", id_print);
                    command.Parameters.AddWithValue("@IdPlastic", id_pl);
                    command.Parameters.AddWithValue("@IdModel", id_list.Text);
                    command.Parameters.AddWithValue("@IdSettings", id_settings.Text);
                    command.Parameters.AddWithValue("@NameProject", Name_project.Text);
                    command.Parameters.AddWithValue("@Author", Author.Text);
                    command.Parameters.AddWithValue("@Date", selectedDate);
                    command.Parameters.AddWithValue("@Comment", Comment.Text);
                    command.Parameters.AddWithValue("@Result", Result.Text);
                    command.ExecuteNonQuery();
                    id_project.Text = Convert.ToString(Convert.ToInt32(id_project.Text) + 1);
                    MessageBox.Show("Проект " + Name_project.Text + " успешно создан");
                    Plastic = "Пластик: " + id_plastic.SelectedItem.ToString();
                    string sqlQuery = "SELECT * FROM Models WHERE id = @models";
                    int model = Convert.ToInt32(id_settings.Text);
                    using (SqlCommand command1 = new SqlCommand(sqlQuery, sqlConnection))
                    {
                        command1.Parameters.AddWithValue("@models", model);
                        SqlDataReader reader = command1.ExecuteReader();
                        while (reader.Read())
                        {
                            Grid grid1 = CreateModelGrid(reader, Author.Text, Plastic, Convert.ToInt32(id_project.Text), Name_project.Text);
                            createdGrid = grid1;
                            mainWindow.AddGridToListBox(createdGrid);
                        }
                        reader.Close();
                        CreateProjectEvent();
                        this.Close();
                    }

                }
            }
        }
        private Grid CreateModelGrid(SqlDataReader reader, string author, string plasticName, int idProjectToFind, string NameProject)
        {
            Grid grid1 = new Grid();
            grid1.Margin = new Thickness(0, 0, 0, 20);
            for (int i = 0; i < 7; i++)
            {
                grid1.RowDefinitions.Add(new RowDefinition());
            }
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            TextBlock TextId = new TextBlock();
            TextId.Text = idProjectToFind.ToString();
            TextId.Visibility = Visibility.Hidden;
            TextBlock textBlock4 = CreateTextBlock("Автор: " + author, 18);
            TextBlock textBlock5 = CreateTextBlock(plasticName, 18);
            TextBlock textBlock1 = CreateTextBlock(reader["Name"].ToString(), 26);
            TextBlock textBlock6 = CreateTextBlock("Название проекта - " + NameProject, 26);
            TextBlock textBlock2 = CreateTextBlock("Функциональное назначение: " + reader["Functional_Purpose"].ToString(), 20);
            TextBlock textBlock3 = CreateTextBlock("Размер_модели: x - " + reader["Size_model(x)"].ToString() + "   y - " + reader["Size_model(y)"].ToString() + "   z - " + reader["Size_model(z)"].ToString(), 18);
            TextBlock textBlock7 = CreateTextBlock("Описание: " + reader["Description"].ToString(), 18);
            Image image = new Image();
            image.Width = 200;
            image.Height = 200;
            byte[] imageData = (byte[])reader["Image"];
            if (imageData != null && imageData.Length > 0)
            {
                BitmapImage imageSource = new BitmapImage();
                imageSource.BeginInit();
                imageSource.StreamSource = new System.IO.MemoryStream(imageData);
                imageSource.EndInit();
                image.Source = imageSource;
            }
            Grid.SetRowSpan(image, 6);
            Grid.SetColumn(textBlock1, 1);
            Grid.SetColumn(TextId, 1);
            Grid.SetColumn(textBlock6, 1);
            Grid.SetColumn(textBlock2, 1);
            Grid.SetColumn(textBlock3, 1);
            Grid.SetColumn(textBlock4, 1);
            Grid.SetColumn(textBlock5, 1);
            Grid.SetColumn(textBlock7, 1);
            Grid.SetRow(textBlock1, 1);
            Grid.SetRow(TextId, 0);
            Grid.SetRow(textBlock6, 0);
            Grid.SetRow(textBlock2, 2);
            Grid.SetRow(textBlock3, 3);
            Grid.SetRow(textBlock4, 4);
            Grid.SetRow(textBlock5, 5);
            Grid.SetRow(textBlock7, 6);
            grid1.Children.Add(image);
            grid1.Children.Add(TextId);
            grid1.Children.Add(textBlock6);
            grid1.Children.Add(textBlock1);
            grid1.Children.Add(textBlock2);
            grid1.Children.Add(textBlock3);
            grid1.Children.Add(textBlock5);
            grid1.Children.Add(textBlock4);
            grid1.Children.Add(textBlock7);
            grid1.Background = new SolidColorBrush(Colors.LightGray);
            return grid1;
        }
        private TextBlock CreateTextBlock(string text, int fontSize)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = text;
            textBlock.FontSize = fontSize;
            textBlock.Padding = new Thickness(20, 10, 0, 0);
            textBlock.Width = 450;
            textBlock.TextWrapping = TextWrapping.Wrap;
            textBlock.HorizontalAlignment = HorizontalAlignment.Left;
            return textBlock;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Creation_Model creation_Model = new Creation_Model();
            creation_Model.Owner = this;
            creation_Model.ShowDialog();

        }
        private void Window_Closed(object sender, EventArgs e)
        {
            this.IsEnabled = true; 
        }
        private void HandleIdSelect(string id)
        {
            id_list.Text = id;
        }
        private void button_selection_Click(object sender, RoutedEventArgs e)
        {
            Selection_model selection_Model = new Selection_model();
            selection_Model.IdSelect += HandleIdSelect;
            selection_Model.ShowDialog();
        }

        // Метод-обработчик события IdSelected
        private void HandleIdSelected(string id)
        {
            id_settings.Text = id;
        }
        private void select_button_Click(object sender, RoutedEventArgs e)
        {
            Settings_Select settingsSelect = new Settings_Select();

            // Подписка на событие IdSelected
            settingsSelect.IdSelected += HandleIdSelected;

            settingsSelect.ShowDialog();
        }
    }
}
