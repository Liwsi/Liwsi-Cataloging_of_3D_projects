using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
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
    /// Логика взаимодействия для Settings_edit.xaml
    /// </summary>
    public partial class Settings_edit : Window
    {
        public Settings_edit()
        {
            InitializeComponent();
        }
        private SqlConnection sqlConnection = null;
        bool check;
        int Model;
        int Plastic;
        private bool dataLoaded = false;
        bool notification = true;
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
            sqlConnection.Open();
            string query = "SELECT Name  FROM Plastics";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                SqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    plastic_name.Items.Add(reader[0]);
                    plastic_name.SelectedIndex = 0;
                }
                reader.Close();
            }
            dataLoaded = true;
            DataGrid_View();
        }
        
        private void DataGrid_View()
        {
            Model = model_type.SelectedIndex + 1;
            Plastic = plastic_name.SelectedIndex + 1;
            string query = "SELECT Property,[Unit of measurement],[Favorite Settings] FROM Property ";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0)
                    {
                        DataTable Table = new DataTable();
                        Table.Columns.Add("Тип настройки", typeof(string));
                        Table.Columns.Add("Свойства", typeof(string));
                        Table.Columns.Add("Значения", typeof(string));
                        Table.Columns.Add("Единица измерения", typeof(string));
                        Table.Columns.Add("Избранное", typeof(string));
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            DataRow row = dataTable.Rows[i];
                            DataRow row1 = Table.NewRow();
                            row1["Свойства"] = row[0].ToString();
                            row1["Единица измерения"] = row[1].ToString();
                            if(row[2].ToString() == "True")
                            {
                                row1["Избранное"] = "\u2713";
                            }
                            else
                            {
                                row1["Избранное"] = "\u2717";
                            }
                            Table.Rows.Add(row1);

                        }
                        string query1 = $"SELECT * FROM Settings WHERE Model_type = {Model} AND Plastic = {Plastic}";
                        using (SqlCommand command1 = new SqlCommand(query1, sqlConnection))
                        {
                            using (SqlDataAdapter adapter1 = new SqlDataAdapter(command1))
                            {
                                DataTable dataTable1 = new DataTable();
                                adapter1.Fill(dataTable1);
                                if (dataTable1.Rows.Count > 0)
                                {
                                    for (int i = 3; i < dataTable1.Columns.Count; i++)
                                    {
                                        DataColumn column = dataTable1.Columns[i];
                                        Table.Rows[i - 3]["Значения"] = dataTable1.Rows[0][column];
                                       
                                    }

                                    string query2 = $"SELECT * FROM Default_Settings WHERE Model_type = {Model} AND Plastic = {Plastic}";
                                    using (SqlCommand command2 = new SqlCommand(query2, sqlConnection))
                                    {
                                        using (SqlDataAdapter adapter2 = new SqlDataAdapter(command2))
                                        {
                                            DataTable dataTable2 = new DataTable();
                                            adapter2.Fill(dataTable2);
                                            if (dataTable2.Rows.Count > 0)
                                            {
                                                for (int i = 3; i < dataTable2.Columns.Count; i++)
                                                {
                                                    DataColumn column = dataTable2.Columns[i];
                                                    if (dataTable2.Rows[0][column].ToString() != Table.Rows[i - 3]["Значения"].ToString() )
                                                    {
                                                        Table.Rows[i - 3]["Тип настройки"] = "▪";
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    Table_settings.ItemsSource = Table.DefaultView;
                                    Table_settings.Items.SortDescriptions.Add(new SortDescription("Избранное", ListSortDirection.Ascending));
                                    Table_settings.Columns[0].IsReadOnly = true;
                                    Table_settings.Columns[0].Header = "";
                                    Table_settings.Columns[1].IsReadOnly = true;
                                    Table_settings.Columns[3].IsReadOnly = true;
                                    Table_settings.Columns[4].IsReadOnly = true;
                                    Table_settings.Columns[2].Width = DataGridLength.Auto;
                                    Table_settings.Columns[1].Width = DataGridLength.Auto;
                                    Table_settings.Columns[0].Width = DataGridLength.Auto;
                                    check = true;
                                }
                                else
                                {
                                  
                                    if (notification)
                                        MessageBox.Show("Настроек для такого типа модели не найдено! Настройки будут загружены из стандартных пресетов");
                                    string query2 = $"SELECT * FROM Default_Settings WHERE Model_type = {Model} AND Plastic = {Plastic}";
                                    using (SqlCommand command2 = new SqlCommand(query2, sqlConnection))
                                    {
                                        using (SqlDataAdapter adapter2 = new SqlDataAdapter(command2))
                                        {
                                            DataTable dataTable2 = new DataTable();
                                            adapter2.Fill(dataTable2);
                                            for (int i = 3; i < dataTable1.Columns.Count; i++)
                                            {
                                                DataColumn column = dataTable2.Columns[i];
                                                Table.Rows[i - 3]["Значения"] = dataTable2.Rows[0][column];
                                            }
                                            Table_settings.ItemsSource = Table.DefaultView;
                                            Table_settings.Items.SortDescriptions.Add(new SortDescription("Избранное", ListSortDirection.Ascending));
                                            Table_settings.Columns[0].IsReadOnly = true;
                                            Table_settings.Columns[0].Header = "";
                                            Table_settings.Columns[1].IsReadOnly = true;
                                            Table_settings.Columns[3].IsReadOnly = true;
                                            Table_settings.Columns[4].IsReadOnly = true;
                                            Table_settings.Columns[2].Width = DataGridLength.Auto;
                                            Table_settings.Columns[1].Width = DataGridLength.Auto;
                                            Table_settings.Columns[0].Width = DataGridLength.Auto;
                                            check = false;
                                        }
                                    }

                                }

                            }
                        }

                    }

                }
            }
        }

        private void Redaction_Button_Click(object sender, RoutedEventArgs e)
        {
           
            Table_settings.Items.SortDescriptions.Clear();
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
            sqlConnection.Open();
            Model = model_type.SelectedIndex + 1;
            Plastic = plastic_name.SelectedIndex + 1;
            List<string> NameColumn = new List<string>(); 
            string namecolumn = "SELECT COLUMN_NAME AS [Имя столбца] FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Settings'";
            using (SqlCommand command = new SqlCommand(namecolumn, sqlConnection))
            {
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    NameColumn.Add(reader[0].ToString());
                }
                reader.Close();
            }
            if (check)
            {
                string query = "";
                for (int i = 0; i < Table_settings.Items.Count-1; i++)
                {
                    var value = NameColumn[i+3];
                    var secondRow = Table_settings.Items[i] as DataRowView;
                    var value2 = secondRow["Значения"];
                    query = query + " " + value + " " + "=" + " " + 'N' +  "'" + value2 + "'"+ ",";
                }
                string queryFinal = query.Remove(query.Length - 1);
                queryFinal = "UPDATE Settings SET " + queryFinal + " WHERE Model_type = @Model AND Plastic = @Plastic";
                using (SqlCommand sqlCommand = new SqlCommand(queryFinal, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@Model", Model);
                    sqlCommand.Parameters.AddWithValue("@Plastic", Plastic);
                    int rows = sqlCommand.ExecuteNonQuery();
                }
                DataGrid_View();
                Table_settings.Items.SortDescriptions.Add(new SortDescription("Избранное", ListSortDirection.Ascending));
                MessageBox.Show("Настройки успешно обновлены!");

            }
            else
            {
                int lastId = 1;
                string query = "SELECT MAX(id) FROM Settings";
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    object result = sqlCommand.ExecuteScalar();
                    lastId = Convert.ToInt32(result) + 1;
                }
                string query1 = "";
                for (int i = 0; i < Table_settings.Items.Count - 1; i++)
                {
                    var value = NameColumn[i + 3];
                    query1 = query1 + value + ",";
                }
                string queryFinal = query1;
                queryFinal = "INSERT INTO  Settings (" + queryFinal + "Id" +"," + "Model_type" + "," + "Plastic" + ")" + " " + "VALUES" + " " + "(";
                for (int i = 0; i < Table_settings.Items.Count - 1; i++)
                {
                    var secondRow = Table_settings.Items[i] as DataRowView;
                    var value2 = secondRow["Значения"];
                    queryFinal = queryFinal + 'N' + "'" + value2 + "'" + ",";
                }
                string queryFinal2 = queryFinal;
                queryFinal2 = queryFinal2 + "@Id" + "," + "@Model_type" + "," + "@Plastic" + ")";
                using (SqlCommand sqlCommand1 = new SqlCommand(queryFinal2, sqlConnection))
                {
                    sqlCommand1.Parameters.AddWithValue("@Id", lastId);
                    sqlCommand1.Parameters.AddWithValue("@Model_type", Model);
                    sqlCommand1.Parameters.AddWithValue("@Plastic", Plastic);
                    int rows = sqlCommand1.ExecuteNonQuery();
                }
                DataGrid_View();
                Table_settings.Items.SortDescriptions.Add(new SortDescription("Избранное", ListSortDirection.Ascending));
                MessageBox.Show("Новые настройки добавлены!");
            }

            
           
        }

        private void Reset_button_Click(object sender, RoutedEventArgs e)
        {
           
            Table_settings.Items.SortDescriptions.Clear();
            if (check)
            {
                sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
                sqlConnection.Open();
                string query = "SELECT * FROM Default_Settings WHERE Plastic=@Plastic AND Model_type=@Model_type";
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@Model_type", Model);
                    sqlCommand.Parameters.AddWithValue("@Plastic", Plastic);
                    SqlDataReader reader = sqlCommand.ExecuteReader();
                    while (reader.Read())
                    {
                        string a = reader[3].ToString();
                        for (int i = 3; i < reader.VisibleFieldCount; i++)
                        {
                            var second = Table_settings.Items[i - 3] as DataRowView;
                            second["Значения"] = reader[i].ToString();
                            second["Тип настройки"] = ""; /// сброс маркеров первого поля Тип настройки
                        }
                        Table_settings.Items.SortDescriptions.Add(new SortDescription("Избранное", ListSortDirection.Ascending));
                        
                        MessageBox.Show("Настройки сброшены до стандартных!");
                    }

                }

            }
            else
                MessageBox.Show("Нельзя сбросить стандартные настройки!");

        }

        private void button_favorite_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Table_settings.SelectedItems.Count; i++)
            {
                var secondRow = Table_settings.SelectedItems[i] as DataRowView;
                var value = secondRow["Свойства"];
                string query = "UPDATE Property SET [Favorite settings] = 'True' WHERE Property = @Property";
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@Property", value);
                    int rows = sqlCommand.ExecuteNonQuery();
                    if (rows > 0)
                        MessageBox.Show("Настройки добавлены в избранные!");
                    else
                        MessageBox.Show("В обновлении настроек произошла ошибка!");
                }

            }
            notification = false;
            DataGrid_View();
        }

        private void button_delete_Click(object sender, RoutedEventArgs e)
        {
            for (int i = 0; i < Table_settings.SelectedItems.Count; i++)
            {
                var secondRow = Table_settings.SelectedItems[i] as DataRowView;
                var value = secondRow["Свойства"];
                string query = "UPDATE Property SET [Favorite settings] = 'False' WHERE Property = @Property";
                using (SqlCommand sqlCommand = new SqlCommand(query, sqlConnection))
                {
                    sqlCommand.Parameters.AddWithValue("@Property", value);
                    int rows = sqlCommand.ExecuteNonQuery();
                    if (rows > 0)
                        MessageBox.Show("Настройки удалены из избранных!");
                    else
                        MessageBox.Show("В обновлении настроек произошла ошибка!");
                }

            }
            notification = false;
            DataGrid_View();
        }

        private void model_type_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            notification = true;
            if (dataLoaded)
                DataGrid_View();
        }

        private void plastic_name_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            notification = true;
            if (dataLoaded)
                DataGrid_View();
        }

        
    }
    
}
