using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml.Linq;
using System.ComponentModel.Composition;
using System.Windows.Controls.Primitives;
using System.Collections;
using System.Reflection.PortableExecutable;
using Microsoft.Win32;
using OfficeOpenXml.Style;
using OfficeOpenXml;


namespace diplom
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private SqlConnection sqlConnection = null;
        int count;
        public void AddGridToListBox(Grid grid)
        {
            List1.Items.Add(grid);
        }

        private Grid CreateModelGrid(SqlDataReader reader, string author, string plasticName, int idProjectToFind, string NameProject)
        {
            Grid grid1 = new Grid();
            grid1.Margin = new Thickness(0, 0, 0, 10);
            for (int i = 0; i < 8; i++)
            {
                grid1.RowDefinitions.Add(new RowDefinition());
            }
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
            grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
            TextBlock TextId = new TextBlock();
            TextId.Text = idProjectToFind.ToString();
            TextId.Visibility = Visibility.Hidden;
            TextBlock textBlock4 = CreateTextBlock(author, 20);
            TextBlock textBlock5 = CreateTextBlock(plasticName, 20);
            TextBlock textBlock1 = CreateTextBlock("Название модели: " + reader["Name"].ToString(), 22);
            TextBlock textBlock6 = CreateTextBlock(NameProject, 28);
            TextBlock textBlock2 = CreateTextBlock("Функциональное назначение: " + reader["Functional_Purpose"].ToString(), 20);
            TextBlock textBlock3 = CreateTextBlock("Размер модели: x - " + reader["Size_model(x)"].ToString() + "   y - " + reader["Size_model(y)"].ToString() + "   z - " + reader["Size_model(z)"].ToString(), 20);
            TextBlock textBlock7 = CreateTextBlock("Описание: " + reader["Description"].ToString(), 20);
            textBlock7.Margin = new Thickness(0, 0, 0, 20);
            textBlock6.HorizontalAlignment = HorizontalAlignment.Center;
            textBlock6.Padding = new Thickness(0,10,0,20);
            CheckBox check = new CheckBox();
           
            check.Content = "Сравнить";
            check.Margin = new Thickness(10, 0, 0, 10);
            check.FontSize = 20;
            check.VerticalContentAlignment = VerticalAlignment.Center;
            Image image = new Image();
            image.Width = 200;
            image.Height = 200;
            image.Margin = new Thickness(10, 80, 10, 0);
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
            Grid.SetRow(check, 7);
            grid1.Children.Add(check);
            grid1.Children.Add(image);
            grid1.Children.Add(TextId);
            grid1.Children.Add(textBlock6);
            grid1.Children.Add(textBlock1);
            grid1.Children.Add(textBlock2);
            grid1.Children.Add(textBlock3);
            grid1.Children.Add(textBlock5);
            grid1.Children.Add(textBlock4);
            grid1.Children.Add(textBlock7);
            grid1.Background = new SolidColorBrush(Color.FromRgb(229, 228, 226));
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

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSpisok();
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
            sqlConnection.Open();
            int index = List1.SelectedIndex;
            if (index == -1)
                MessageBox.Show("Выберите проект из списка, который хотите удалить!");

            else
            {
                MessageBoxResult messageBoxResult = MessageBox.Show("Вы действительно хотите удалить этот проект?", "Удаление проекта", MessageBoxButton.YesNo);
                if (messageBoxResult == MessageBoxResult.Yes)
                {
                    foreach (object item in List1.SelectedItems)
                    {
                        Grid grid = (Grid)item;
                        foreach (var element in grid.Children)
                        {
                            if (element is TextBlock)
                            {
                                TextBlock idProject = (TextBlock)element;
                                string id = idProject.Text;
                                string deleteQuery = "DELETE FROM Projects WHERE id_project = @id";
                                using (SqlCommand command = new SqlCommand(deleteQuery, sqlConnection))
                                {
                                    command.Parameters.AddWithValue("@id", id);
                                    int rowsAffected = command.ExecuteNonQuery();
                                }
                                List1.Items.RemoveAt(index);
                                return;
                            }
                        }

                    }
                }
                
                
            }
        }
        string id_P = "";
        private void ButtonModify_Click(object sender, RoutedEventArgs e)
        {
            int index = List1.SelectedIndex;
            if (index == -1)
                MessageBox.Show("Выберите проект из списка, который хотите редактировать!");
            else
            {
                foreach (object item in List1.SelectedItems)
                {
                    Grid grid = (Grid)item;
                    foreach (var element in grid.Children)
                    {
                        if (element is TextBlock)
                        {
                            TextBlock idProject = (TextBlock)element;
                            id_P = idProject.Text;
                            Redaction_Window redaction_Window = new Redaction_Window();
                            redaction_Window.Id_P = id_P;
                            redaction_Window.CreateProjectEvent += CreateProject;
                            redaction_Window.ShowDialog();
                            return;
                        }
                    }

                }
                
            }
           
        }
        public void LoadSpisok()
        {
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
            {
                sqlConnection.Open();
                string query = "SELECT Name  FROM Plastics";
                using (SqlCommand command3 = new SqlCommand(query, sqlConnection))
                {
                    SqlDataReader reader = command3.ExecuteReader();

                    while (reader.Read())
                    {
                        Name_plast.Items.Add(reader[0].ToString());
                        Name_plast.SelectedIndex = 0;
                    }
                    reader.Close();
                }


            }
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
            {
                sqlConnection.Open();

                string select = "SELECT id_project, id_plastic, Name_project, id_model, Author, plast.Name AS PlasticName, model.* " +
                                     "FROM Projects " +
                                     "JOIN Plastics plast ON id_plastic = plast.id " +
                                     "JOIN Models model ON id_model = model.id";

                using (SqlCommand command = new SqlCommand(select, sqlConnection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id_project = Convert.ToInt32(reader["id_project"]);
                            int id_model = Convert.ToInt32(reader["id_model"]);
                            string author = reader["Author"].ToString();
                            string name_project = reader["Name_project"].ToString();
                            string plasticName = reader["PlasticName"].ToString();
                            Grid grid1 = CreateModelGrid(reader, "Автор: " + author, "Пластик: " + plasticName, id_project, "Название проекта - " + name_project);
                            List1.Items.Add(grid1);
                            List1.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                            List1.Padding = new Thickness(0);
                        }
                    }
                }
            }

        }
        public void CreateProject()
        {
            List1.Items.Clear();
            LoadSpisok();
        }
        private void OpenRedactionWindow_Click(object sender, RoutedEventArgs e)
        {
          
        }
        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            Creation_Window createWindow = new Creation_Window(this);
            createWindow.CreateProjectEvent += CreateProject;
            createWindow.ShowDialog();
        }

        private void ButtonModelModify_Click(object sender, RoutedEventArgs e)
        {
            int index = List1.SelectedIndex;
            if (index == -1)
                MessageBox.Show("Выберите проект из списка, в котором хотите отредактировать модель!");
            else
            {
                foreach (object item in List1.SelectedItems)
                {
                    Grid grid = (Grid)item;
                    foreach (var element in grid.Children)
                    {
                        if (element is TextBlock)
                        {
                            TextBlock idProject = (TextBlock)element;
                            id_P = idProject.Text;
                            Redaction_Model redaction_Model = new Redaction_Model();
                            redaction_Model.Id_project = id_P;
                            redaction_Model.CreateProjectEvent += CreateProject;
                            redaction_Model.ShowDialog();
                            return;
                        }
                    }

                }

            }
           
        }

        private void ButtonSettings_Click(object sender, RoutedEventArgs e)
        {
            Settings_edit settings_Edit = new Settings_edit();
            settings_Edit.Show();
        }

        private void bClear_Click(object sender, RoutedEventArgs e)
        {
            
            string Name = Name_proj.Text;
            string selectQuery = "SELECT id_project, id_plastic, Name_project, id_model, Author, plast.Name AS PlasticName, model.* " +
                               "FROM Projects " +
                               "JOIN Plastics plast ON id_plastic = plast.id " +
                               "JOIN Models model ON id_model = model.id " +
                               "WHERE Name_project = @Name";
            Filter(selectQuery, Name);
           
        }
        private void filter_author_Click(object sender, RoutedEventArgs e)
        {
            string Name = Author_p.Text;
            string selectQuery = "SELECT id_project, id_plastic, Name_project, id_model, Author, plast.Name AS PlasticName, model.* " +
                               "FROM Projects " +
                               "JOIN Plastics plast ON id_plastic = plast.id " +
                               "JOIN Models model ON id_model = model.id " +
                               "WHERE Author = @Name";
            Filter(selectQuery, Name);
        }
        private void filter_plastic_Click(object sender, RoutedEventArgs e)
        {
            string Name = Name_plast.SelectedValue.ToString();
            string selectQuery = "SELECT id_project, id_plastic, Name_project, id_model, Author, plast.Name AS PlasticName, model.* " +
                               "FROM Projects " +
                               "JOIN Plastics plast ON id_plastic = plast.id " +
                               "JOIN Models model ON id_model = model.id " +
                               "WHERE plast.Name = @Name";
            Filter(selectQuery, Name);
        }
        private void filter_model_Click(object sender, RoutedEventArgs e)
        {
           string Name =  name_model.Text;
            string selectQuery = "SELECT id_project, id_plastic, Name_project, id_model, Author, plast.Name AS PlasticName, model.* " +
                              "FROM Projects " +
                              "JOIN Plastics plast ON id_plastic = plast.id " +
                              "JOIN Models model ON id_model = model.id " +
                              "WHERE model.Name = @Name";
            Filter(selectQuery, Name);
        }

        public void Filter(string query,string Names)
        {
            List1.Items.Clear();
            using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
            {
                sqlConnection.Open();

                string select = query;
                string Name = Names;

                using (SqlCommand command = new SqlCommand(select, sqlConnection))
                {
                    command.Parameters.AddWithValue("@Name", Name);
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id_project = Convert.ToInt32(reader["id_project"]);
                            int id_model = Convert.ToInt32(reader["id_model"]);
                            string author = reader["Author"].ToString();
                            string name_project = reader["Name_project"].ToString();
                            string plasticName = reader["PlasticName"].ToString();
                            Grid grid1 = CreateModelGrid(reader, "Автор: " + author, "Пластик: " + plasticName, id_project, "Название проекта - " + name_project);
                            List1.Items.Add(grid1);
                            List1.HorizontalContentAlignment = HorizontalAlignment.Stretch;
                        }
                    }
                }
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List1.Items.Clear();
            Name_proj.Clear();
            Name_plast.Items.Clear();
            name_model.Clear();
            Author_p.Clear();
            LoadSpisok();
        }

        private void button_upload_Click(object sender, RoutedEventArgs e)
        {
            
            int index = List1.SelectedIndex;
            if (index == -1)
                MessageBox.Show("Выберите проект из списка, который вы хотите выгрузить!");
            else
            {
                foreach (object item in List1.SelectedItems)
                {
                    Grid grid = (Grid)item;
                    foreach (var element in grid.Children)
                    {
                        if (element is TextBlock)
                        {
                            TextBlock idProject = (TextBlock)element;
                            id_P = idProject.Text;
                            Data data = new Data();
                            id_P = idProject.Text;
                            data.Id_P = id_P;
                            data.ShowDialog();
                            return;
                        }
                    }

                }

            }
            
        }

        private void ButtonAddModel_Click(object sender, RoutedEventArgs e)
        {
            Creation_Model creation_Model = new Creation_Model();
            creation_Model.Show();
        }

        private void button_info_Click(object sender, RoutedEventArgs e)
        {
            Info info = new Info();
            info.Show();    
        }

        private void button_duplicate_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult messageBoxResult = MessageBox.Show("Вы действительно хотите дублировать проект?", "Дублирование проекта", MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                string Name_p = "";
                int index = List1.SelectedIndex;
                if (index == -1)
                {
                    MessageBox.Show("Выберите проект из списка, который хотите дублировать!");
                }
                else
                {
                    foreach (object item in List1.SelectedItems)
                    {
                        Grid grid = (Grid)item;
                        foreach (var element in grid.Children)
                        {
                            if (element is TextBlock)
                            {
                                TextBlock idProject = (TextBlock)element;
                                id_P = idProject.Text;
                                break;
                            }
                        }
                    }
                }

                string Project_name = "";
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
                {
                    sqlConnection.Open();
                    string query = "SELECT Name_project FROM Projects WHERE Id_project = @id_P";
                    using (SqlCommand command = new SqlCommand(query, sqlConnection))
                    {
                        command.Parameters.AddWithValue("@id_P", id_P);
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Name_p = reader["Name_project"].ToString();
                            }
                        }
                    }
                }

                for (int i = 1; ; i++)
                {
                    Project_name = Name_p + " " + "копия" + " " + i.ToString();
                    using (SqlConnection sqlConnection1 = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
                    {
                        sqlConnection1.Open();
                        string query1 = "SELECT COUNT(*) FROM Projects WHERE Name_project = @Project_name";
                        using (SqlCommand command1 = new SqlCommand(query1, sqlConnection1))
                        {
                            command1.Parameters.AddWithValue("@Project_name", Project_name);
                            int result = (int)command1.ExecuteScalar();
                            if (result == 0)
                            {
                                break;
                            }
                        }
                    }
                }

                List<string> list = new List<string>();
                foreach (object item in List1.SelectedItems)
                {
                    Grid grid = (Grid)item;
                    foreach (var element in grid.Children)
                    {
                        if (element is TextBlock)
                        {
                            TextBlock idProject = (TextBlock)element;
                            list.Add(idProject.Text);
                        }
                    }
                }
                Grid grid1 = new Grid();
                grid1.Margin = new Thickness(0, 0, 0, 20);
                for (int i = 0; i < 7; i++)
                {
                    grid1.RowDefinitions.Add(new RowDefinition());
                }
                grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Auto) });
                grid1.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                TextBlock TextId = new TextBlock();
                TextId.Text = id_P;
                TextId.Visibility = Visibility.Hidden;
                TextBlock textBlock4 = CreateTextBlock(list[6], 18);
                TextBlock textBlock5 = CreateTextBlock(list[5], 18);
                TextBlock textBlock1 = CreateTextBlock(list[2], 26);
                TextBlock textBlock6 = CreateTextBlock("Название проекта - " + Project_name, 26);
                TextBlock textBlock2 = CreateTextBlock(list[3], 20);
                TextBlock textBlock3 = CreateTextBlock(list[4], 18);
                TextBlock textBlock7 = CreateTextBlock(list[7], 18);
                Image image = new Image();
                image.Width = 200;
                image.Height = 200;
                foreach (object item in List1.SelectedItems)
                {
                    Grid grid = (Grid)item;
                    foreach (var element in grid.Children)
                    {
                        if (element is Image)
                        {
                            Image image1 = (Image)element;
                            image.Source = image1.Source;
                        }
                    }

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
                List1.Items.Add(grid1);
                int maxId = 0;
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
                {
                    connection.Open();
                    string getMaxIdQuery = "SELECT MAX(Id_project) FROM Projects";
                    using (SqlCommand getMaxIdCommand = new SqlCommand(getMaxIdQuery, connection))
                    {
                        object maxIdResult = getMaxIdCommand.ExecuteScalar();
                        if (maxIdResult != DBNull.Value)
                        {
                            maxId = Convert.ToInt32(maxIdResult);
                        }
                    }
                }
                int existingProjectId = Convert.ToInt32(id_P);
                string selectQuery = "SELECT * FROM Projects WHERE Id_project = @Id_project";
                using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
                {
                    connection.Open();
                    using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@Id_project", existingProjectId);
                        using (SqlDataReader reader = selectCommand.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                int id_printer = (int)reader["id_printer"];
                                int id_plastic = (int)reader["id_plastic"];
                                int id_model = (int)reader["id_model"];
                                int id_settings = (int)reader["id_settings"];
                                string Name_project = reader["Name_project"].ToString();
                                string Author = reader["Author"].ToString();
                                DateTime Date = (DateTime)reader["Date"];
                                string Comment = reader["Comment"].ToString();
                                string Result = reader["Result"].ToString();
                                reader.Close();
                                int newProjectId = maxId + 1;
                                string newProjectName = Project_name;
                                string insertQuery = @"INSERT INTO Projects (Id_project, id_printer, id_plastic, id_model, id_settings, Name_project, Author, Date, Comment, Result)
                           VALUES (@Id_project, @id_printer, @id_plastic, @id_model, @id_settings, @Name_project, @Author, @Date, @Comment, @Result)";
                                using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                                {
                                    insertCommand.Parameters.AddWithValue("@Id_project", newProjectId);
                                    insertCommand.Parameters.AddWithValue("@id_printer", id_printer);
                                    insertCommand.Parameters.AddWithValue("@id_plastic", id_plastic);
                                    insertCommand.Parameters.AddWithValue("@id_model", id_model);
                                    insertCommand.Parameters.AddWithValue("@id_settings", id_settings);
                                    insertCommand.Parameters.AddWithValue("@Name_project", newProjectName);
                                    insertCommand.Parameters.AddWithValue("@Author", Author);
                                    insertCommand.Parameters.AddWithValue("@Date", Date);
                                    insertCommand.Parameters.AddWithValue("@Comment", Comment);
                                    insertCommand.Parameters.AddWithValue("@Result", Result);
                                    insertCommand.ExecuteNonQuery();
                                }
                                MessageBox.Show("Проект успешно продублирован!");
                            }
                        }
                    }
                }
                List1.Items.Clear();
                LoadSpisok();
            }
        }

        private void button_compare_Click(object sender, RoutedEventArgs e)
        {
            List<string> strings = new List<string>();
            foreach (object item in List1.Items)
            {
                Grid grid = (Grid)item;
                foreach (var element in grid.Children)
                {
                    if (element is CheckBox)
                    {
                        CheckBox check1 = (CheckBox)element;
                        if (check1.IsChecked == true)
                        {
                            foreach (var element1 in grid.Children)
                            {
                                if (element1 is TextBlock)
                                {
                                    TextBlock idProject = (TextBlock)element1;
                                    strings.Add(idProject.Text);
                                    break;
                                }
                            }
                           
                        }
                    }
                }
            }
            if (strings.Count > 1)
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                    Title = "Save Excel File",
                    FileName = "Сравнение"
                };
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.NonCommercial;


                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("GridData");
                    int count = 1;
                    for (int i = 0; i < strings.Count; i++)
                    {
                        string project_id = strings[i];
                        List<string> project_properties = new List<string>();
                        List<string> project_value = new List<string>();
                        sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
                        sqlConnection.Open();
                        string query_project = @"
         SELECT Projects.*, Plastics.Name AS PlasticName, Printers.Name AS PrinterName
         FROM Projects 
         JOIN Plastics ON Projects.id_plastic = Plastics.Id 
         JOIN Printers ON Projects.id_printer = Printers.Id
         WHERE Projects.Id_project = @id";
                        using (SqlCommand command_p = new SqlCommand(query_project, sqlConnection))
                        {
                            command_p.Parameters.AddWithValue("@id", project_id);
                            SqlDataReader reader = command_p.ExecuteReader();

                            while (reader.Read())
                            {
                                project_value.Add(reader[0].ToString());
                                project_value.Add(reader[11].ToString());
                                project_value.Add(reader[10].ToString());
                                project_value.Add(reader[3].ToString());
                                project_value.Add(reader[4].ToString());
                                project_value.Add(reader[5].ToString());
                                project_value.Add(reader[6].ToString());
                                project_value.Add(reader[7].ToString());
                                project_value.Add(reader[8].ToString());
                                project_value.Add(reader[9].ToString());
                                project_properties.Add("ID проекта");
                                project_properties.Add("Принтер");
                                project_properties.Add("Пластик");
                                project_properties.Add("ID модели");
                                project_properties.Add("ID настроек");
                                project_properties.Add("Навание");
                                project_properties.Add("Автор");
                                project_properties.Add("Дата");
                                project_properties.Add("Комментарий");
                                project_properties.Add("Результат");
                            }
                            reader.Close();
                        }
                        string query = "SELECT Property, [Unit of measurement] FROM Property";
                        List<string> settings_property = new List<string>();
                        List<string> settings_uofm = new List<string>();
                        List<string> settings_value = new List<string>();
                        using (SqlCommand command = new SqlCommand(query, sqlConnection))
                        {
                            using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                            {
                                DataSet dataSet = new DataSet();
                                adapter.Fill(dataSet);

                                foreach (DataRow row in dataSet.Tables[0].Rows)
                                {
                                    string property = row["Property"].ToString();
                                    settings_property.Add(property);
                                }
                                foreach (DataRow row in dataSet.Tables[0].Rows)
                                {
                                    string unitOfMeasurement = row["Unit of measurement"].ToString();
                                    settings_uofm.Add(unitOfMeasurement);
                                }
                                string query1 = "SELECT * FROM Settings WHERE Id=@id";
                                using (SqlCommand command1 = new SqlCommand(query1, sqlConnection))
                                {
                                    command1.Parameters.AddWithValue("@id", project_value[4]);
                                    using (SqlDataAdapter adapter1 = new SqlDataAdapter(command1))
                                    {
                                        DataSet dataSet1 = new DataSet();
                                        adapter1.Fill(dataSet1);

                                        foreach (DataRow row in dataSet1.Tables[0].Rows)
                                        {
                                            for (int j = 3; j < dataSet1.Tables[0].Columns.Count; j++)
                                            {
                                                string value = row[j].ToString();
                                                settings_value.Add(value);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        string query_m = "SELECT *  FROM Models WHERE Id = @id ";
                        List<string> model_property = new List<string>();
                        List<string> model_value = new List<string>();
                        using (SqlCommand command_m = new SqlCommand(query_m, sqlConnection))
                        {
                            command_m.Parameters.AddWithValue("@id", project_value[3]);
                            SqlDataReader reader = command_m.ExecuteReader();
                            while (reader.Read())
                            {
                                model_property.Add("ID модели");
                                model_property.Add("Название");
                                model_property.Add("Назначение");
                                model_property.Add("Размер (x,y,z)");
                                model_property.Add("Описание");
                                model_value.Add(reader[0].ToString());
                                model_value.Add(reader[1].ToString());
                                model_value.Add(reader[2].ToString());
                                model_value.Add(reader[3].ToString() + " : " + reader[4].ToString() + " : " + reader[5].ToString());
                                model_value.Add(reader[8].ToString());
                            }
                            reader.Close();
                        }
                        ExcelRange mergeRange = worksheet.Cells[1, count, 1, count + 1];
                        mergeRange.Merge = true;
                        mergeRange.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        mergeRange.Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        worksheet.Cells[1, count].Value = "Проект" + " - " + project_value[5];
                        worksheet.Cells[2, count].Value = "Свойства";
                        worksheet.Cells[2, count + 1].Value = "Значения";
                        for (int j = 0; j < project_properties.Count; j++)
                        {
                            worksheet.Cells[3 + j, count].Value = project_properties[j].ToString();
                        }
                        for (int k = 0; k < project_value.Count; k++)
                        {
                            worksheet.Cells[3 + k, count + 1].Value = project_value[k].ToString();
                        }
                        int startRow = 2;
                        int endRow = startRow + Math.Max(project_properties.Count, project_value.Count);
                        int startColumn = count;
                        int endColumn = startColumn + 1;
                        ExcelRange tableRange = worksheet.Cells[startRow, startColumn, endRow, endColumn];
                        var border = tableRange.Style.Border;
                        border.Bottom.Style = ExcelBorderStyle.Thin;
                        border.Top.Style = ExcelBorderStyle.Thin;
                        border.Left.Style = ExcelBorderStyle.Thin;
                        border.Right.Style = ExcelBorderStyle.Thin;
                        worksheet.Cells[14, count].Value = "Модель";
                        worksheet.Cells[15, count].Value = "Свойства";
                        worksheet.Cells[15, count + 1].Value = "Значения";
                        for (int f = 0; f < model_property.Count; f++)
                        {
                            worksheet.Cells[16 + f, count].Value = model_property[f].ToString();
                        }
                        for (int g = 0; g < model_value.Count; g++)
                        {
                            worksheet.Cells[16 + g, count + 1].Value = model_value[g].ToString();
                        }
                        startRow = 15;
                        endRow = startRow + Math.Max(model_property.Count, model_value.Count);
                        startColumn = count;
                        endColumn = startColumn + 1;
                        ExcelRange tableRange1 = worksheet.Cells[startRow, startColumn, endRow, endColumn];
                        var border1 = tableRange1.Style.Border;
                        border1.Bottom.Style = ExcelBorderStyle.Thin;
                        border1.Top.Style = ExcelBorderStyle.Thin;
                        border1.Left.Style = ExcelBorderStyle.Thin;
                        border1.Right.Style = ExcelBorderStyle.Thin;

                        worksheet.Cells[22, count].Value = "Настройки";
                        worksheet.Cells[23, count].Value = "Свойства";
                        worksheet.Cells[23, count + 1].Value = "Значения";
                        worksheet.Cells[23, count + 2].Value = "Единица измерения";
                        for (int d = 0; d < settings_property.Count; d++)
                        {
                            worksheet.Cells[24 + d, count].Value = settings_property[d].ToString();
                        }
                        for (int v = 0; v < settings_value.Count; v++)
                        {
                            worksheet.Cells[24 + v, count + 1].Value = settings_value[v].ToString();
                        }
                        for (int c = 0; c < settings_uofm.Count; c++)
                        {
                            worksheet.Cells[24 + c, count + 2].Value = settings_uofm[c].ToString();
                        }
                        startRow = 23;
                        endRow = startRow + Math.Max(Math.Max(settings_property.Count, settings_value.Count), settings_uofm.Count);
                        startColumn = count;
                        endColumn = startColumn + 2;

                        ExcelRange tableRange2 = worksheet.Cells[startRow, startColumn, endRow, endColumn];
                        var border2 = tableRange2.Style.Border;
                        border2.Bottom.Style = ExcelBorderStyle.Thin;
                        border2.Top.Style = ExcelBorderStyle.Thin;
                        border2.Left.Style = ExcelBorderStyle.Thin;
                        border2.Right.Style = ExcelBorderStyle.Thin;
                        count += 5;

                    }

                    ExcelRange range = worksheet.Cells[worksheet.Dimension.Address];
                    range.Style.Font.Size = 14;
                    ExcelRange range1 = worksheet.Cells["1:2"]; 
                    ExcelRange range2 = worksheet.Cells["14:15"]; 
                    ExcelRange range3 = worksheet.Cells["22:23"];
                    range1.Style.Font.Bold = true;
                    range2.Style.Font.Bold = true;
                    range3.Style.Font.Bold = true;
                    worksheet.Cells.AutoFitColumns();

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                        excelPackage.SaveAs(excelFile);
                        MessageBox.Show($"Данные проектов экспортированы по указанному пути: {excelFile.FullName}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Количество проектов для сравнения должно быть больше 1!");
            }
            

        }
    }

}
