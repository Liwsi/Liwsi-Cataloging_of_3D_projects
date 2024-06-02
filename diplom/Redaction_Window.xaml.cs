using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Shapes;

namespace diplom
{
    /// <summary>
    /// Логика взаимодействия для Redaction_Window.xaml
    /// </summary>
    public partial class Redaction_Window : Window
    {
        private SqlConnection sqlConnection = null;
        public delegate void CreateProjectHandler();
        public event CreateProjectHandler CreateProjectEvent;
        public string Id_P { get; set; }  

        public Redaction_Window()
        {
            InitializeComponent();

        }
        string ID_model = "";
        public void AddValue(string id, string name)
        {
            ID_model = id;
            id_list.Text = name;
        }
 
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString);
            sqlConnection.Open();
            id_project.Text = Id_P;
            string query3 = "SELECT 'Plastics' AS TableName, Name FROM Plastics " +
                "UNION ALL " +
                "SELECT 'Printers' AS TableName, Name FROM Printers ";
            using (SqlCommand command3 = new SqlCommand(query3, sqlConnection))
            {
                SqlDataReader reader = command3.ExecuteReader();

                while (reader.Read())
                {
                    string tableName = reader.GetString(reader.GetOrdinal("TableName"));
                    string name = reader.GetString(reader.GetOrdinal("Name"));
                    if (tableName == "Printers")
                    {
                        id_printers.Items.Add(name);
                    }
                    if (tableName == "Plastics")
                    {
                        id_plastic.Items.Add(name);
                    }
                }
                reader.Close();
            }
            string sqlQuery1 = "SELECT * FROM Projects WHERE Id_project = @project";
            int project = Convert.ToInt32(Id_P);
            
            using (SqlCommand command1 = new SqlCommand(sqlQuery1, sqlConnection))
            {
                command1.Parameters.AddWithValue("@project", project);
                SqlDataReader reader = command1.ExecuteReader();
                while (reader.Read())
                {
                    Name_project.Text = reader[5].ToString();
                    id_settings.Text = reader[4].ToString();
                    id_printers.SelectedIndex = Convert.ToInt32(reader[1].ToString()) -1;
                    id_plastic.SelectedIndex = Convert.ToInt32(reader[2].ToString()) -1 ;
                    Author.Text = reader[6].ToString(); 
                    Date.Text = reader[7].ToString();
                    Comment.Text = reader[8].ToString();
                    Result.Text = reader[9].ToString(); 
                    id_list.Text = reader[3].ToString();

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

                string updateQuery = "UPDATE Projects SET " +
                            "id_printer = @IdPrinter, " +
                            "id_plastic = @IdPlastic, " +
                            "id_model = @IdModel, " +
                            "id_settings = @IdSettings, " +
                            "Name_project = @NameProject, " +
                            "Author = @Author, " +
                            "Date = @Date, " +
                            "Comment = @Comment, " +
                            "Result = @Result " +
                            "WHERE Id_project = @IdProject";
                DateTime selectedDate = Date.SelectedDate ?? DateTime.MinValue;
                using (SqlConnection sqlConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString))
                using (SqlCommand sqlCommand = new SqlCommand(updateQuery, sqlConnection))
                {
                    sqlCommand.Parameters.Add(new SqlParameter("@IdProject", Id_P));
                    sqlCommand.Parameters.Add(new SqlParameter("@IdPrinter", id_printers.SelectedIndex+1));
                    sqlCommand.Parameters.Add(new SqlParameter("@IdPlastic", id_plastic.SelectedIndex + 1));
                    sqlCommand.Parameters.Add(new SqlParameter("@IdModel", id_list.Text));
                    sqlCommand.Parameters.Add(new SqlParameter("@IdSettings", id_settings.Text));
                    sqlCommand.Parameters.Add(new SqlParameter("@NameProject", Name_project.Text));
                    sqlCommand.Parameters.Add(new SqlParameter("@Author", Author.Text));
                    sqlCommand.Parameters.Add(new SqlParameter("@Date", selectedDate));
                    sqlCommand.Parameters.Add(new SqlParameter("@Comment", Comment.Text));
                    sqlCommand.Parameters.Add(new SqlParameter("@Result", Result.Text));
                    sqlConnection.Open();
                    int rowsAffected = sqlCommand.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Проект успешно отредактирован!");
                    }
                }
                CreateProjectEvent();
                this.Close();
            }
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

        private void HandleIdSelected(string id)
        {
            id_settings.Text = id;
        }
        private void select_button_Click(object sender, RoutedEventArgs e)
        {
            Settings_Select settingsSelect = new Settings_Select();
            settingsSelect.IdSelected += HandleIdSelected;
            settingsSelect.ShowDialog();
        }
    }
}
