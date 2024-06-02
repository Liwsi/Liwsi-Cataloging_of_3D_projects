using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для Settings_Select.xaml
    /// </summary>
    public partial class Settings_Select : Window
    {
        private SqlConnection sqlConnection = null;
        public Settings_Select()
        {
            InitializeComponent();
        }
        public event Action<string> IdSelected;

        // Метод вызова события
        private void OnIdSelected(string id)
        {
            IdSelected?.Invoke(id);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT " +
                  "Settings.Id, " +
                  "Model_Type.Type AS Model_Type, " +
                  "Plastics.Name AS Plastic " +
                  "FROM " +
                  "Settings " +
                  "JOIN Model_Type ON Settings.Model_Type = Model_Type.Id " +
                  "JOIN Plastics ON Settings.Plastic = Plastics.Id";

                DataTable Table = new DataTable();
                Table.Columns.Add("Id", typeof(string));
                Table.Columns.Add("Тип модели", typeof(string));
                Table.Columns.Add("Пластик", typeof(string));

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            string modelType = reader.GetString(reader.GetOrdinal("Model_Type"));
                            string plastic = reader.GetString(reader.GetOrdinal("Plastic"));
                            DataRow row = Table.NewRow();
                            row["Id"] = id;
                            row["Тип модели"] = modelType;
                            row["Пластик"] = plastic;
                            Table.Rows.Add(row);
                        }
                    }
                    Settings.ItemsSource = Table.DefaultView;
                    Settings.Columns[0].Width = 100;
                    Settings.Columns[1].Width = 300;
                    Settings.Columns[2].Width = 300;

                }
            }
        }

        private void button_selection_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = Settings;
            DataRowView rowView = (DataRowView)dataGrid.SelectedItem;
            DataRow row = rowView.Row;
            string id = row[0].ToString();
            OnIdSelected(id);
            MessageBox.Show("Настройки успешно выбраны!");
            this.Close();

        }
    }
}
