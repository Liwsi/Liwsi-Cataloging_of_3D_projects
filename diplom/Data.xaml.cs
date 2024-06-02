using Microsoft.Win32;
using OfficeOpenXml.Drawing;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;
using OfficeOpenXml.Style;




namespace diplom
{
    /// <summary>
    /// Логика взаимодействия для Data.xaml
    /// </summary>
    public partial class Data : Window
    {
        public string Id_P { get; set; }
        private SqlConnection sqlConnection = null;
        public Data()
        {
            InitializeComponent();
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string project_id = Id_P;
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
                    id_project.Text = reader[0].ToString();
                    id_model.Text = reader[3].ToString();   
                    id_setting.Text = reader[4].ToString();
                    name_project.Text = reader[5].ToString();
                    Author.Text = reader[6].ToString();
                    data_p.Text = reader[7].ToString();
                    comment_p.Text = reader[8].ToString();
                    result_p.Text = reader[9].ToString();
                    id_plastic.Text = reader[10].ToString();
                    id_printer.Text = reader[11].ToString();

                }
                reader.Close();
            }
            string query = "SELECT Property,[Unit of measurement] FROM Property ";
            using (SqlCommand command = new SqlCommand(query, sqlConnection))
            {
                using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                {
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    if (dataTable.Rows.Count > 0)
                    {
                        DataTable Table = new DataTable();
                        Table.Columns.Add("Свойства", typeof(string));
                        Table.Columns.Add("Значения", typeof(string));
                        Table.Columns.Add("Единица измерения", typeof(string));
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            DataRow row = dataTable.Rows[i];
                            DataRow row1 = Table.NewRow();
                            row1["Свойства"] = row[0].ToString();
                            row1["Единица измерения"] = row[1].ToString();
                            Table.Rows.Add(row1);
                        }
                        string query1 = "SELECT * FROM Settings WHERE Id=@id";
                        using (SqlCommand command1 = new SqlCommand(query1, sqlConnection))
                        {
                             command1.Parameters.AddWithValue("@id", id_setting.Text);
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
                                    Data_setting.ItemsSource = Table.DefaultView;
                                    Data_setting.Columns[0].IsReadOnly = true;
                                    Data_setting.Columns[2].IsReadOnly = true;
                                    Data_setting.Columns[1].IsReadOnly = true;
                                    Data_setting.Columns[2].Width = DataGridLength.Auto;
                                    Data_setting.Columns[1].Width = DataGridLength.Auto;
                                    Data_setting.Columns[0].Width = DataGridLength.Auto;
                                }
                               

                            }
                        }

                    }

                }
            }
            string query_m = "SELECT *  FROM Models WHERE Id = @id ";
            using (SqlCommand command_m = new SqlCommand(query_m, sqlConnection))
            {
                command_m.Parameters.AddWithValue("@id", id_model.Text);
                SqlDataReader reader = command_m.ExecuteReader();
                while (reader.Read())
                {
                    id_m.Text = reader[0].ToString();
                    name_m.Text = reader[1].ToString();
                    purpose_m.Text = reader[2].ToString();
                    Size.Text = reader[3].ToString() + " : " + reader[4].ToString() + " : " + reader[5].ToString();
                    description_m.Text = reader[8].ToString();
                    byte[] imageData = (byte[])reader[6];
                    if (imageData != null && imageData.Length > 0)
                    {
                        BitmapImage imageSource = new BitmapImage();
                        imageSource.BeginInit();
                        imageSource.StreamSource = new System.IO.MemoryStream(imageData);
                        imageSource.EndInit();
                        System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                        image.Source = imageSource;
                        image.Width = 150;
                        image.Height = 150;
                        image.VerticalAlignment = VerticalAlignment.Center;
                      

                        ListBoxItem listBoxItem = new ListBoxItem();
                        listBoxItem.Content = image;
                        image_list.Items.Add(listBoxItem);
                        
                    }

                }
                reader.Close();
            }
        }

        private void upload_button_Click(object sender, RoutedEventArgs e)
        {
            ExportExcelFile();
        }

        private void ExportExcelFile()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Save Excel File",
                FileName = name_project.Text
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                ExcelPackage excelPackage = new ExcelPackage();
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("GridData");
                Grid grid = Projects;
                worksheet.Cells[1, 1].Value = "Проект";
                worksheet.Cells[2, 1].Value = "Свойства";
                worksheet.Cells[2, 2].Value = "Значения";
                for (int i = 0; i < grid.Children.Count; i++)
                {
                    UIElement element = grid.Children[i];
                    int row = Grid.GetRow(element);
                    int col = Grid.GetColumn(element);

                    if (element is TextBlock textBlock)
                    {
                        worksheet.Cells[row + 3, col + 1].Value = textBlock.Text;
                    }
                }
                grid = Model;
                worksheet.Cells[1, 4].Value = "Модель";
                worksheet.Cells[2, 4].Value = "Свойства";
                worksheet.Cells[2, 5].Value = "Значения";
                for (int i = 0; i < grid.Children.Count; i++)
                {
                    UIElement element = grid.Children[i];
                    int row = Grid.GetRow(element);
                    int col = Grid.GetColumn(element);
                    if (element is TextBlock textBlock)
                    {
                        worksheet.Cells[row + 3, col + 4].Value = textBlock.Text;
                    }
                }
                DataGrid dataGrid = Data_setting;
                worksheet.Cells[1, 7].Value = "Настройки";
                worksheet.Cells[2, 7].Value = "Свойства";
                worksheet.Cells[2, 8].Value = "Значения";
                worksheet.Cells[2, 9].Value = "Единица измерения";
                for (int i = 0; i < dataGrid.Items.Count - 1; i++)
                {
                    DataRowView rowView = (DataRowView)dataGrid.Items[i];
                    DataRow row = rowView.Row;
                    worksheet.Cells[i + 3, 7].Value = row["Свойства"];
                    worksheet.Cells[i + 3, 8].Value = row["Значения"];
                    worksheet.Cells[i + 3, 9].Value = row["Единица измерения"];
                }
                for (int i = 0; i < image_list.Items.Count; i++)
                {
                    ListBoxItem listBoxItem = (ListBoxItem)image_list.Items[i];
                    System.Windows.Controls.Image image = (System.Windows.Controls.Image)listBoxItem.Content;
                    BitmapImage bitmapImage = (BitmapImage)image.Source;

                    byte[] imageBytes;
                    JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        encoder.Save(memoryStream);
                        imageBytes = memoryStream.ToArray();
                    }

                    string imagePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(saveFileDialog.FileName), $"{name_m.Text}.jpg");
                    File.WriteAllBytes(imagePath, imageBytes);
                }


                ExcelRange range = worksheet.Cells["A1:G1"];
                range.Style.Font.Size = 16;
                range.Style.Font.Bold = true;
                worksheet.Cells["A1:B1"].Merge = true;
                worksheet.Cells["A1:B1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["D1:E1"].Merge = true;
                worksheet.Cells["D1:E1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells["G1:I1"].Merge = true;
                worksheet.Cells["G1:I1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                ExcelRange range1 = worksheet.Cells["A2:I105"];
                FileInfo excelFile = new FileInfo(saveFileDialog.FileName);
                range1.Style.Font.Size = 14;
                ExcelRange range2 = worksheet.Cells["A2:B12"];
                range2.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range2.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range2.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range2.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ExcelRange range3 = worksheet.Cells["D2:E8"];
                range3.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range3.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range3.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range3.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                ExcelRange range4 = worksheet.Cells["G2:I105"];
                range4.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                range4.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                range4.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                range4.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                worksheet.Cells.AutoFitColumns();
                excelPackage.SaveAs(excelFile);
                ExportModelFile(saveFileDialog.FileName);
                MessageBox.Show($"Данные проекта и модели экспортированы по указанному пути: {excelFile.FullName}");
            }
        }

        private void ExportModelFile(string filePath)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["CatalogDB"].ConnectionString;
            string sqlQuery = "SELECT File_model FROM models WHERE Id = @ModeldId";
            string ModeldId = id_m.Text;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                using (SqlCommand command = new SqlCommand(sqlQuery, connection))
                {
                    command.Parameters.AddWithValue("@ModeldId", ModeldId);
                    using (SqlDataReader reader = command.ExecuteReader(CommandBehavior.SequentialAccess))
                    {
                        if (reader.Read())
                        {
                            string fileName = System.IO.Path.GetFileNameWithoutExtension(filePath) + ".gcode";
                            string directoryPath = System.IO.Path.GetDirectoryName(filePath);
                            string gcodeFilePath = System.IO.Path.Combine(directoryPath, fileName);

                            using (FileStream filestream = new FileStream(gcodeFilePath, FileMode.Create))
                            {
                                int dataIndex = reader.GetOrdinal("File_model");
                                const int bufferSize = 1024;
                                byte[] buffer = new byte[bufferSize];
                                long bytesRead;
                                long fieldOffset = 0;

                                while ((bytesRead = reader.GetBytes(dataIndex, fieldOffset, buffer, 0, bufferSize)) > 0)
                                {
                                    filestream.Write(buffer, 0, (int)bytesRead);
                                    fieldOffset += bytesRead;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
