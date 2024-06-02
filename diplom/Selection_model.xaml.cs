using System;
using System.Collections;
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
    /// Логика взаимодействия для Selection_model.xaml
    /// </summary>
    public partial class Selection_model : Window
    {
        public Selection_model()
        {
            InitializeComponent();
        }



        private SqlConnection sqlConnection = null;
        public event Action<string> IdSelect;

        // Метод вызова события
        private void OnIdSelected(string id)
        {
            IdSelect?.Invoke(id);
        }

        private void button_selection_Click(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = Models;
            DataRowView rowView = (DataRowView)dataGrid.SelectedItem;
            DataRow row = rowView.Row;
            string id = row[0].ToString();
            OnIdSelected(id);
            MessageBox.Show("Модель успешно выбрана!");
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (SqlConnection connection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
            {
                connection.Open();

                string sqlQuery = "SELECT Id, Name FROM Models";

                DataTable Table = new DataTable();
                Table.Columns.Add("Id", typeof(string));
                Table.Columns.Add("Модель", typeof(string));

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(reader.GetOrdinal("Id"));
                            string model = reader.GetString(reader.GetOrdinal("Name"));
                            DataRow row = Table.NewRow();
                            row["Id"] = id;
                            row["Модель"] = model;
                            Table.Rows.Add(row);
                        }
                    }
                    Models.ItemsSource = Table.DefaultView;
                    Models.Columns[0].Width = 100;
                    Models.Columns[1].Width = 400;

                }
            }
        }

    }
}
