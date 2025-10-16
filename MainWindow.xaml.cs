using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using static WpfTestCase.MainWindow;
using System.Data;
using Npgsql;
using Dapper;
using Newtonsoft.Json;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing;
using Xdr = DocumentFormat.OpenXml.Drawing.Spreadsheet;
using A = DocumentFormat.OpenXml.Drawing;
using C = DocumentFormat.OpenXml.Drawing.Charts;
using X14 = DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Drawing.Spreadsheet;
using DocumentFormat.OpenXml.Office2016.Excel;


namespace WpfTestCase
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                // เคลียร์ช่องที่อยู่ไฟล์
                FilePathTextBox.Text = string.Empty;

                // เคลียร์ช่องค้นหา
                Txt01.Text = string.Empty;

                // เคลียร์ DataGrid
                DgLoadExcel.ItemsSource = null;
                //DgLoadExcel.Visibility = Visibility.Collapsed;

                // รีเซ็ต Progress Bar
                ProgressBar.Value = 0;
                ProgressText.Text = "0%";

                lblTotal.Content = "";
                lblPass.Content = "";
                lblFailed.Content = "";

                // โฟกัสกลับไปที่ช่องค้นหา
                Txt01.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"เกิดข้อผิดพลาดขณะเคลียร์ข้อมูล: {ex.Message}",
                              "ข้อผิดพลาด",
                              MessageBoxButton.OK,
                              MessageBoxImage.Error);
            }
        }
        private async void Button01_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                // string orderId = this.Txt01.Text.StartsWith("O", StringComparison.OrdinalIgnoreCase) ? this.Txt01.Text.ToString() : "O" + this.Txt01.Text.ToString();
                string orderId = this.Txt01.Text;

                if (!string.IsNullOrEmpty(orderId))
                {

                    List<Order> lisOrder = new List<Order>();
                    Order order = new Order();
                    order.OrderId = orderId;
                    lisOrder.Add(order);

                    // สร้าง Progress Reporter
                    var progress = new Progress<int>(percent =>
                    {
                        ProgressBar.Value = percent;
                        ProgressText.Text = $"{percent}%";
                    });

                    var processData = await ProcessEventsStatus(lisOrder.ToList(), progress);

                    // ต้องการ progress bar wpf c#
                    lblTotal.Content = processData.Count.ToString();
                    int countlblPass = processData.Count(item => item.StatusCode >= 1 && item.StatusCode <= 7);
                    lblPass.Content = countlblPass.ToString();
                    int countlblFailed = (processData.Count() - countlblPass);
                    lblFailed.Content = countlblFailed.ToString();

                    DgLoadExcel.ItemsSource = processData;
                    DgLoadExcel.Visibility = Visibility.Visible;
                }
                else
                {
                    MessageBox.Show($"Code is empty. Please enter a value.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            // openFileDialog.Filter = "xlsx files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            openFileDialog.Filter = "Excel Files (*.xlsx)|*.xlsx";

            if (openFileDialog.ShowDialog() == true)
            {
                FilePathTextBox.Text = openFileDialog.FileName;

                try
                {
                    string filePath = openFileDialog.FileName;


                    if (System.IO.Path.GetExtension(filePath).ToLower() != ".xlsx")
                    {
                        MessageBox.Show("กรุณาเลือกไฟล์ .xlsx เท่านั้น");
                        return;
                    }

                    // สร้าง Progress Reporter
                    var progress = new Progress<int>(percent =>
                    {
                        ProgressBar.Value = percent;
                        ProgressText.Text = $"{percent}%";
                    });

                    var rawDataExcels = await GetOrderList(filePath);
                    var rawDataExcel = rawDataExcels.ToList();

                    var processData = await ProcessEventsStatus(rawDataExcel.ToList(), progress);

                    lblTotal.Content = processData.Count.ToString();
                    int countlblPass = processData.Count(item => item.StatusCode >= 1 && item.StatusCode <= 7);
                    lblPass.Content = countlblPass.ToString();
                    int countlblFailed = (processData.Count() - countlblPass);
                    lblFailed.Content = countlblFailed.ToString();


                    DgLoadExcel.ItemsSource = processData;
                    DgLoadExcel.Visibility = Visibility.Visible;

                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error reading file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        private async Task<ObservableCollection<Order>> GetOrderList(string filePath)
        {
            var orders = new ObservableCollection<Order>();

            try
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Open(filePath, false))
                {
                    WorkbookPart workbookPart = document.WorkbookPart;

                    // หา Sheet ชื่อ "Detail" (ไม่สนใจตัวพิมพ์เล็กใหญ่และช่องว่าง)
                    Sheet theSheet = workbookPart.Workbook.Descendants<Sheet>().FirstOrDefault(static s => string.Equals(s.Name, "Detail", StringComparison.OrdinalIgnoreCase));

                    if (theSheet == null)
                    {
                        throw new Exception("ไม่พบ Sheet ชื่อ 'Detail' ในไฟล์นี้");
                    }

                    WorksheetPart worksheetPart = (WorksheetPart)(workbookPart.GetPartById(theSheet.Id));
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();

                    Row headerRow = sheetData.Elements<Row>().FirstOrDefault();
                    foreach (Row row in sheetData.Elements<Row>().Skip(1))
                    {

                        var cells = row.Elements<Cell>().ToList();
                        string _orderId = GetCellValue(cells[0], workbookPart);
                        //string orderId = _orderId.StartsWith("O", StringComparison.OrdinalIgnoreCase) ? _orderId : "O" + _orderId;
                        string orderId = _orderId;

                        orders.Add(new Order
                        {
                            RunNo = "",
                            Number = "",
                            MM = "",
                            TransactionDate = DateTime.Now,
                            OrderId = orderId,
                            TicketNo = "",
                            IsSameDay = "",
                            Delivery = "",
                            Status = "",
                            OrderError = "",
                            Pos = "",
                            RootCause = "",
                            Error = "",
                            Job = "",
                            CaseIrNo = "",
                            User = "",
                            CaseReviews = ""
                        });

                        //orders.Add(new Order
                        //{
                        //    RunNo = GetCellValue(cells[0], workbookPart),
                        //    Number = GetCellValue(cells[1], workbookPart),
                        //    MM = GetCellValue(cells[2], workbookPart),
                        //    TransactionDate = DateTime.Parse(GetCellValue(cells[3], workbookPart)),
                        //    OrderId = "O" + GetCellValue(cells[4], workbookPart),
                        //    TicketNo = GetCellValue(cells[5], workbookPart),
                        //    IsSameDay = GetCellValue(cells[6], workbookPart),
                        //    Delivery = GetCellValue(cells[7], workbookPart),
                        //    Status = GetCellValue(cells[8], workbookPart),
                        //    OrderError = GetCellValue(cells[9], workbookPart),
                        //    Pos = GetCellValue(cells[10], workbookPart),
                        //    RootCause = GetCellValue(cells[11], workbookPart),
                        //    Error = GetCellValue(cells[12], workbookPart),
                        //    Job = GetCellValue(cells[13], workbookPart),
                        //    CaseIrNo = GetCellValue(cells[14], workbookPart),
                        //    User = GetCellValue(cells[15], workbookPart),
                        //    CaseReviews = "Test"
                        //});
                    }
                }
            }
            catch (Exception ex)
            {
                // ควรจัดการ error อย่างเหมาะสม เช่น logging
                MessageBox.Show($"เกิดข้อผิดพลาดในการอ่านไฟล์: {ex.Message}");
            }

            return await Task.FromResult(orders);
        }
        private string GetCellValue(Cell cell, WorkbookPart workbookPart)
        {
            if (cell == null || cell.CellValue == null)
                return string.Empty;

            string value = cell.CellValue.Text;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                SharedStringTablePart stringTable = workbookPart.GetPartsOfType<SharedStringTablePart>().First();
                value = stringTable.SharedStringTable.ElementAt(int.Parse(value)).InnerText;
            }

            return value;
        }
        private async Task<List<TbEvents>> ConnectionDB(string orderNo)
        {

            var lstEvents = new List<TbEvents>();
            try
            {
                string connectionString = "Host=127.0.0.1;Port=5432;Database=prd;Username=postgres;Password=BHwmeKABN2KpoxL8";

                using (IDbConnection db = new NpgsqlConnection(connectionString))
                {
                    try
                    {
                        string orderId = orderNo.StartsWith("O", StringComparison.OrdinalIgnoreCase) ? orderNo : "O" + orderNo;
                        db.Open();
                        var parameters = new DynamicParameters();
                        parameters.Add("Value", orderId);
                        //parameters.Add("Limit", 10);
                        //select * from  events e where e.value = 'O5827423'
                        //lstEvents = db.Query<TbEvents>("SELECT * FROM events limit 10").ToList();
                        lstEvents = (List<TbEvents>)await db.QueryAsync<TbEvents>("SELECT * FROM events WHERE value = @Value and type in ('QUEUE','CUSTOMER','ORDER') order by createdate  desc", parameters);
                    }
                    catch (Exception ex)
                    {
                        lstEvents = new List<TbEvents>();
                        MessageBox.Show($"Not Connect PLSQL Concect To GCP", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch
            {

            }
            return await Task.FromResult(lstEvents);
        }
        public async Task<string> EnsureStartsWithO(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                MessageBox.Show("Code is empty. Please enter a value.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                return await Task.FromResult("");
            }

            return await Task.FromResult(input.StartsWith("O", StringComparison.OrdinalIgnoreCase) ? input : "O" + input);
        }
        public async Task<PatternTbEvents> SequenceEventsStructureOne(List<TbEvents> events)
        {
            PatternTbEvents pattern = new PatternTbEvents();
            List<TbEvents> lis = new List<TbEvents>();
            //int i = 1;

            var e = events;
            if (e.Any())
            {
                var count = e.Count;

                #region Pattern
                //Pattern seq 1
                if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "QUEUE")
                {
                    pattern.SeqPattern = 1;
                    pattern.SeqCount = 6;
                    pattern.boxA = 5;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 1;
                    pattern.boxE = 0;
                }
                //Pattern seq 2 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "QUEUE")
                {
                    pattern.SeqPattern = 2;
                    pattern.SeqCount = 7;
                    pattern.boxA = 6;
                    pattern.boxB = 4;
                    pattern.boxC = 3;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 3 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "QUEUE")
                {
                    pattern.SeqPattern = 3;
                    pattern.SeqCount = 7;
                    pattern.boxA = 6;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 1;
                    pattern.boxE = 0;
                }
                //Pattern seq 4 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 4;
                    pattern.SeqCount = 8;
                    pattern.boxA = 7;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 1;
                    pattern.boxE = 0;
                }
                //Pattern seq 5 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "QUEUE"
                     && e[7].Type == "CUSTOMER"
                    && e[8].Type == "QUEUE")
                {
                    pattern.SeqPattern = 5;
                    pattern.SeqCount = 9;
                    pattern.boxA = 8;
                    pattern.boxB = 6;
                    pattern.boxC = 5;
                    pattern.boxD = 1;
                    pattern.boxE = 0;
                }
                //Pattern seq 6 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "QUEUE"
                    && e[7].Type == "QUEUE"
                    && e[8].Type == "CUSTOMER"
                    && e[9].Type == "QUEUE")
                {
                    pattern.SeqPattern = 6;
                    pattern.SeqCount = 10;
                    pattern.boxA = 9;
                    pattern.boxB = 7;
                    pattern.boxC = 6;
                    pattern.boxD = 1;
                    pattern.boxE = 0;
                }
                //Pattern seq 7 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "QUEUE")
                {
                    pattern.SeqPattern = 7;
                    pattern.SeqCount = 7;
                    pattern.boxA = 6;
                    pattern.boxB = 4;
                    pattern.boxC = 3;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 8 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "QUEUE"
                    && e[7].Type == "CUSTOMER"
                    && e[8].Type == "CUSTOMER"
                    && e[9].Type == "QUEUE")
                {
                    pattern.SeqPattern = 8;
                    pattern.SeqCount = 10;
                    pattern.boxA = 9;
                    pattern.boxB = 6;
                    pattern.boxC = 5;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 9 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "QUEUE"
                    && e[7].Type == "CUSTOMER"
                    && e[8].Type == "CUSTOMER"
                    && e[9].Type == "QUEUE")
                {
                    pattern.SeqPattern = 9;
                    pattern.SeqCount = 10;
                    pattern.boxA = 9;
                    pattern.boxB = 6;
                    pattern.boxC = 5;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 10 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "CUSTOMER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "QUEUE")
                {
                    pattern.SeqPattern = 10;
                    pattern.SeqCount = 10;
                    pattern.boxA = 6;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 11 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "CUSTOMER"
                    && e[8].Type == "QUEUE")
                {
                    pattern.SeqPattern = 11;
                    pattern.SeqCount = 9;
                    pattern.boxA = 8;
                    pattern.boxB = 5;
                    pattern.boxC = 4;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 12 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "CUSTOMER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "QUEUE")
                {
                    pattern.SeqPattern = 12;
                    pattern.SeqCount = 6;
                    pattern.boxA = 5;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 13 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "QUEUE")
                {
                    pattern.SeqPattern = 13;
                    pattern.SeqCount = 7;
                    pattern.boxA = 6;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 14 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "QUEUE"
                    && e[7].Type == "QUEUE"
                    && e[8].Type == "QUEUE"
                    && e[9].Type == "QUEUE"
                    && e[10].Type == "CUSTOMER"
                    && e[11].Type == "QUEUE")
                {
                    pattern.SeqPattern = 14;
                    pattern.SeqCount = 12;
                    pattern.boxA = 11;
                    pattern.boxB = 9;
                    pattern.boxC = 8;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 15 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 15;
                    pattern.SeqCount = 12;
                    pattern.boxA = 7;
                    pattern.boxB = 4;
                    pattern.boxC = 3;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 16 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "QUEUE")
                {
                    pattern.SeqPattern = 16;
                    pattern.SeqCount = 7;
                    pattern.boxA = 6;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 17 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 17;
                    pattern.SeqCount = 8;
                    pattern.boxA = 7;
                    pattern.boxB = 3;
                    pattern.boxC = 2;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 18 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "CUSTOMER"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "QUEUE")
                {
                    pattern.SeqPattern = 18;
                    pattern.SeqCount = 7;
                    pattern.boxA = 6;
                    pattern.boxB = 2;
                    pattern.boxC = 1;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 19 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "CUSTOMER"
                    && e[4].Type == "CUSTOMER"
                    && e[5].Type == "QUEUE")
                {
                    pattern.SeqPattern = 19;
                    pattern.SeqCount = 6;
                    pattern.boxA = 5;
                    pattern.boxB = 2;
                    pattern.boxC = 1;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 20 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "ORDER"
                    && e[3].Type == "QUEUE"// r //C
                    && e[4].Type == "QUEUE"// i
                    && e[5].Type == "QUEUE"// r //B
                    && e[6].Type == "QUEUE"// i 
                    && e[7].Type == "QUEUE"// i 
                    && e[8].Type == "CUSTOMER"
                    && e[9].Type == "QUEUE")// A
                {
                    pattern.SeqPattern = 20;
                    pattern.SeqCount = 10;
                    pattern.boxA = 9;
                    pattern.boxB = 7;
                    pattern.boxC = 6;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 21 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "CUSTOMER"
                    && e[4].Type == "QUEUE")
                {
                    pattern.SeqPattern = 21;
                    pattern.SeqCount = 5;
                    pattern.boxA = 4;
                    pattern.boxB = 2;
                    pattern.boxC = 1;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 22 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 22;
                    pattern.SeqCount = 8;
                    pattern.boxA = 7;
                    pattern.boxB = 5;
                    pattern.boxC = 4;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 23 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 23;
                    pattern.SeqCount = 8;
                    pattern.boxA = 7;
                    pattern.boxB = 5;
                    pattern.boxC = 4;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 24 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "ORDER"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 24;
                    pattern.SeqCount = 8;
                    pattern.boxA = 7;
                    pattern.boxB = 5;
                    pattern.boxC = 4;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 25 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "ORDER"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 24;
                    pattern.SeqCount = 8;
                    pattern.boxA = 7;
                    pattern.boxB = 5;
                    pattern.boxC = 4;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 26 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "ORDER"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "QUEUE"
                    && e[6].Type == "QUEUE"
                    && e[7].Type == "QUEUE"
                    && e[8].Type == "CUSTOMER"
                    && e[9].Type == "QUEUE")
                {
                    pattern.SeqPattern = 26;
                    pattern.SeqCount = 10;
                    pattern.boxA = 9;
                    pattern.boxB = 7;
                    pattern.boxC = 6;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 27 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "QUEUE"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "QUEUE")
                {
                    pattern.SeqPattern = 27;
                    pattern.SeqCount = 8;
                    pattern.boxA = 7;
                    pattern.boxB = 4;
                    pattern.boxC = 3;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                //Pattern seq 28 todo
                else if (e[0].Type == "ORDER"
                    && e[1].Type == "ORDER"
                    && e[2].Type == "QUEUE"
                    && e[3].Type == "QUEUE"
                    && e[4].Type == "QUEUE"
                    && e[5].Type == "CUSTOMER"
                    && e[6].Type == "CUSTOMER"
                    && e[7].Type == "CUSTOMER"
                    && e[8].Type == "QUEUE")
                {
                    pattern.SeqPattern = 28;
                    pattern.SeqCount = 9;
                    pattern.boxA = 8;
                    pattern.boxB = 4;
                    pattern.boxC = 3;
                    pattern.boxD = 2;
                    pattern.boxE = 1;
                }
                #endregion

                int i = 0;
                int seqCount = pattern.SeqCount;
                foreach (TbEvents item in events)
                {
                    if (i < pattern.SeqCount)
                    {
                        item.Seq = seqCount;
                        lis.Add(item);
                    }
                    else
                    {
                        break;
                    }
                    seqCount--;
                    i++;
                }
                pattern.tbEvents = lis;
            }

            return await Task.FromResult(pattern); ;
        }
        private async Task<List<Order>> ProcessEventsStatus(List<Order> order, IProgress<int> progress)
        {
            string orid = "";
            try
            {
                progress?.Report(0);
                if (order.Any())
                {
                    int i = 1;
                    foreach (var e in order)
                    {
                        orid = e.OrderId;
                        if (!string.IsNullOrEmpty(e.OrderId))
                        {

                            List<TbEvents> lstEvents = await ConnectionDB(e.OrderId);
                            if (lstEvents.Any())
                            {
                                try
                                {
                                    var dataSeqEvents = await SequenceEventsStructureOne(lstEvents);
                                    //var dataSeq = dataSeqEvents.tbEvents.OrderBy(x => x.Seq).ToList();
                                    var dataSeq = dataSeqEvents.tbEvents.ToList();
                                    if (dataSeq.Any())
                                    {
                                        // หาแพตเทอว่าอยู่ในรูปแบบ box แบบไหนก่อน
                                        JsonDSRequest? bReq = JsonConvert.DeserializeObject<JsonDSRequest>(dataSeq[dataSeqEvents.boxB].Req) ?? null;
                                        JsonDSResponse? bResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxB].Resp) ?? null;
                                        JsonDSResponse? aResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxA].Resp) ?? null;
                                        JsonDSResponse? cResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxC].Resp) ?? null;


                                        if ((bResp?.ErrorMsg ?? "") != "")
                                        {
                                            e.Error = bResp?.ErrorMsg ?? "";
                                        }
                                        else
                                        {
                                            e.Error = aResp?.ErrorMsg ?? "";
                                        }


                                        CaseType caseType = new CaseType();
                                        #region 1. Server DS Down ชั่วคราว
                                        caseType = await TempServerDSDown(lstEvents);
                                        if (caseType.StatusCase)
                                        {
                                            e.CaseReviews = caseType.CaseTypeReviews;
                                            e.StatusCase = true;
                                            e.StatusCode = 1;
                                            e.RootCause = caseType.RootCause;
                                        }
                                        #endregion

                                        #region 7. Store imports
                                        if (!caseType.StatusCase)
                                        {

                                            if (e.Error.Contains("shipping.Store"))
                                            {
                                                //e.CaseReviews = "DC lead time";
                                                e.CaseReviews = "Store imports";
                                                e.StatusCase = true;
                                                e.StatusCode = 7;
                                                caseType.StatusCase = true;
                                                e.RootCause = caseType.RootCause;
                                            }

                                        }
                                        #endregion

                                        #region 2.logic Stock หน้า web คำนวนผิด
                                        if (!caseType.StatusCase)
                                        {
                                            caseType = await WebStockLogicError(bReq, aResp);
                                            if (caseType.StatusCase)
                                            {
                                                e.CaseReviews = caseType.CaseTypeReviews;
                                                e.StatusCase = true;
                                                e.StatusCode = 2;
                                                e.RootCause = caseType.RootCause;
                                            }
                                        }
                                        #endregion

                                        #region 3. Stock ds หมดระหว่างจองคิว
                                        if (!caseType.StatusCase)
                                        {
                                            caseType = await StockDSOutDuringQueue(bReq, aResp, cResp);
                                            if (caseType.StatusCase)
                                            {
                                                e.CaseReviews = caseType.CaseTypeReviews;
                                                e.StatusCase = true;
                                                e.StatusCode = 3;
                                                e.RootCause = caseType.RootCause;
                                            }
                                        }
                                        #endregion

                                        #region 4. Capa เป็น 0 หน้า web ปล่อยซื้อได้
                                        if (!caseType.StatusCase)
                                        {
                                            caseType = await WebPurchaseAllowedZeroCapa(bReq, aResp);
                                            if (caseType.StatusCase)
                                            {
                                                e.CaseReviews = caseType.CaseTypeReviews;
                                                e.StatusCase = true;
                                                e.StatusCode = 4;
                                                e.RootCause = caseType.RootCause;
                                            }
                                        }
                                        #endregion

                                        #region 5. Capa ds  เป็น 0 ไม่สามารถจองคิวได้
                                        if (!caseType.StatusCase)
                                        {
                                            caseType = await QueueBlockedZeroCapaDS(bReq, aResp, cResp);
                                            if (caseType.StatusCase)
                                            {
                                                e.CaseReviews = caseType.CaseTypeReviews;
                                                e.StatusCase = true;
                                                e.StatusCode = 5;
                                                e.RootCause = caseType.RootCause;
                                            }
                                        }
                                        #endregion

                                        #region 6. Capa ds  มี Stock  มี จองคิวไม่ได้
                                        if (!caseType.StatusCase)
                                        {
                                            caseType = await QueueBlockedDespiteStockCapaDS(bReq, aResp, cResp);
                                            if (caseType.StatusCase)
                                            {
                                                e.CaseReviews = caseType.CaseTypeReviews;
                                                e.StatusCase = true;
                                                e.StatusCode = 6;
                                                e.RootCause = caseType.RootCause;
                                            }
                                        }
                                        #endregion

                                        #region 8. Special case 
                                        if (!caseType.StatusCase)
                                        {
                                            e.CaseReviews = "Special case";
                                            e.StatusCase = true;
                                            e.StatusCode = 99;
                                            e.RootCause = caseType.RootCause;
                                        }
                                        #endregion

                                    }
                                    else
                                    {
                                        //CaseType caseType = new CaseType();
                                        //#region 1. Server DS Down ชั่วคราว
                                        //caseType = await TempServerDSDown(lstEvents);
                                        //e.CaseReviews = caseType.CaseTypeReviews;
                                        //#endregion

                                        //if (!caseType.StatusCase)
                                        {
                                            e.CaseReviews = "Ignore Pattern Flow";
                                            e.StatusCase = true;
                                            e.StatusCode = 99;
                                            //e.RootCause = "Ignore Pattern Flow";
                                        }

                                    }
                                }
                                catch
                                {
                                    //order = new List<Order>();
                                    e.CaseReviews = "Special case";
                                    e.StatusCase = true;
                                    e.StatusCode = 99;
                                    e.OrderId = e.OrderId;
                                    e.Error = "Error : ProcessEventsStatus";
                                }
                            }
                            else
                            {
                                //order = new List<Order>();
                                //e.OrderId = e.OrderId;
                            }
                        }

                        // อัปเดต Progress (คำนวณเปอร์เซ็นต์)
                        int percentComplete = (int)((i / (double)order.Count()) * 100);

                        if (percentComplete <= 0 && i == 1)
                        {
                            percentComplete = 100;
                        }
                        progress?.Report(percentComplete);

                        // จำลองการหน่วงเวลา (ถ้าจำเป็น)
                        //await Task.Delay(10);
                        i++;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fx: ProcessEventsStatus" + "Error : " + ex.Message.ToString(), "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            }
            return await Task.FromResult(order);
        }

        //1. Server DS Down ชั่วคราว
        //1. Server DS Down ชั่วคราว
        public async Task<CaseType> TempServerDSDown(List<TbEvents> lstEvents)
        {
            CaseType caseType = new CaseType();
            try
            {
                List<TbEvents> res = lstEvents.Where(e => e.HttpStatus.ToString() != "200").ToList();
                if (res.Any())
                {
                    caseType.StatusCase = true;
                    caseType.CaseTypeReviews = "Server DS Down ชั่วคราว";
                    var data = res.FirstOrDefault(x => x.HttpStatus != "200");
                    caseType.RootCause = $"RowId:{data.Id.ToString()} Response:{data.Resp}";
                }
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        //2. logic Stock หน้า web คำนวนผิด  //O5544345//O5598901//5620571//5615499//5652193
        //2. logic Stock หน้า web คำนวนผิด  //O5544345//O5598901//5620571//5615499//5652193
        public async Task<CaseType> WebStockLogicError(JsonDSRequest bReq, JsonDSResponse aResp)
        {
            CaseType caseType = new CaseType();
            try
            {
                var insufficientItems = new List<string>();

                // รวม ReserveDataItems จากทุก Rs (กรณีมีหลายวัน)
                var allResponseItems = new List<ResponseReserveDataItem>();
                if (aResp.InquiryRs?.ReserveDataItems != null)
                    allResponseItems.AddRange(aResp.InquiryRs.ReserveDataItems);
                if (aResp.InquirySameDayRs?.ReserveDataItems != null)
                    allResponseItems.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                if (aResp.InquiryNextDayRs?.ReserveDataItems != null)
                    allResponseItems.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);
                if (aResp.InquiryExpressRs?.ReserveDataItems != null)
                    allResponseItems.AddRange(aResp.InquiryExpressRs.ReserveDataItems);

                // Loop ใน request
                foreach (var reqGroup in bReq.ReserveDataItems)
                {
                    if (new[] { "N", "S", "X", "F" }.Contains(reqGroup.QStyle))
                    {
                        foreach (var reqItem in reqGroup.DataItems)
                        {
                            var artNo = reqItem.ArtNo;
                            var reqQty = double.TryParse(reqItem.Qty, out var parsedQty) ? parsedQty : 0;

                            var FlagPremium = reqItem.FlagPremium;
                            // หา response item ที่ ArtNo ตรงกัน
                            //var matchingRespItem = allResponseItems
                            //    .Where(r => r.QStyle == reqGroup.QStyle)
                            //    .SelectMany(r => r.DataItems)
                            //    .FirstOrDefault(d => d.ArtNo == artNo);

                            var matchingRespItems = allResponseItems
                           .Where(r => r.QStyle == reqGroup.QStyle)
                           .SelectMany(r => r.DataItems)
                           .Where(d => d.ArtNo == artNo)
                           .ToList();

                            // bool foundMatching = false; 

                            foreach (var item in matchingRespItems)
                            {
                                if (item != null)
                                {
                                    if (item.StockQty != null)
                                    {
                                        double respStockQty = 0;
                                        if (item.StockQty is string sqStr)
                                            double.TryParse(sqStr, out respStockQty);
                                        else if (item.StockQty is double sqDouble)
                                            respStockQty = sqDouble;
                                        else if (item.StockQty is int sqInt)
                                            respStockQty = sqInt;

                                        if (respStockQty < reqQty)
                                        {
                                            insufficientItems.Add($"QStyle:{reqGroup.QStyle} ArtNo:{artNo} | Stock:{respStockQty} < Request:{reqQty}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }


                if (insufficientItems.Any())
                {
                    caseType.StatusCase = true;
                    caseType.CaseTypeReviews = "logic Stock หน้า web คำนวนผิด";
                    string result = string.Join(Environment.NewLine, insufficientItems);
                    caseType.RootCause = result;
                }
                return caseType;
            }
            catch
            {

            }
            return await Task.FromResult(caseType);
        }

        //3. Stock ds หมดระหว่างจองคิว//O6021351//O6029970
        //3. Stock ds หมดระหว่างจองคิว//O6021351//O6029970
        public async Task<CaseType> StockDSOutDuringQueue(JsonDSRequest bReq, JsonDSResponse aResp, JsonDSResponse cResp)
        {
            CaseType caseType = new CaseType();
            var insufficientItems = new List<string>();
            try
            {
                if (bReq.ReserveDataItems.Any())
                {
                    var aBoxResp = new List<ResponseReserveDataItem>();
                    if (aResp.InquiryRs?.ReserveDataItems != null)
                        aBoxResp.AddRange(aResp.InquiryRs.ReserveDataItems);
                    if (aResp.InquirySameDayRs?.ReserveDataItems != null)
                        aBoxResp.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                    if (aResp.InquiryNextDayRs?.ReserveDataItems != null)
                        aBoxResp.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);
                    if (aResp.InquiryExpressRs?.ReserveDataItems != null)
                        aBoxResp.AddRange(aResp.InquiryExpressRs.ReserveDataItems);


                    var cBoxResp = new List<ResponseReserveDataItem>();
                    if (cResp.InquiryRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquiryRs.ReserveDataItems);
                    if (cResp.InquirySameDayRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquirySameDayRs.ReserveDataItems);
                    if (cResp.InquiryNextDayRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquiryNextDayRs.ReserveDataItems);
                    if (cResp.InquiryExpressRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquiryExpressRs.ReserveDataItems);


                    foreach (var item in bReq.ReserveDataItems)
                    {
                        var qStyle = item.QStyle;

                        foreach (var dataItems in item.DataItems)
                        {
                            var artNo = dataItems.ArtNo;

                            double reqQty = 0;
                            if (dataItems.Qty != null)
                            {
                                var qtyStr = dataItems.Qty.ToString();
                                if (!string.IsNullOrEmpty(qtyStr))
                                {
                                    double.TryParse(qtyStr, out reqQty);
                                }
                            }

                            // A Resp: ต้องมี QStyle + ArtNo + StockQty >= Qty
                            bool foundInA = aBoxResp.Any(q =>
                            q.QStyle == qStyle &&
                            q.DataItems.Any(d =>
                                d.ArtNo == artNo &&
                                d.StockQty is double stockQty &&
                                stockQty >= reqQty
                            ));

                            // C Resp: ต้องมี QStyle + ArtNo + StockQty == 0
                            bool foundInC = cBoxResp.Any(q =>
                                q.QStyle == qStyle &&
                                q.DataItems.Any(d =>
                                    d.ArtNo == artNo &&
                                    d.StockQty is double stockQty &&
                                    stockQty == 0
                                ));

                            if ((foundInA && foundInC))
                            {
                                insufficientItems.Add($"QStyle:{qStyle} ArtNo:{artNo}");
                                caseType.StatusCase = true;
                                caseType.CaseTypeReviews = "Stock ds หมดระหว่างจองคิว";
                                string result = string.Join(Environment.NewLine, insufficientItems);
                                caseType.RootCause = result;
                                return await Task.FromResult(caseType);
                            }
                        }
                    }
                }


            }
            catch { }
            return await Task.FromResult(caseType);
        }

        //4. Capa เป็น 0 หน้า web ปล่อยซื้อได้
        //ข้อ 4.ถามคิว Capa เป็น 0 หน้าเว็ปปล่อยซื้อได้//O5920710//O6020838//O6028021
        public async Task<CaseType> WebPurchaseAllowedZeroCapa(JsonDSRequest bReq, JsonDSResponse aResp)
        {
            CaseType caseType = new CaseType();
            var insufficientItems = new List<string>();

            try
            {
                var bBoxReq = new List<ReserveDataItems>();
                if (bReq.ReserveDataItems != null)
                    bBoxReq.AddRange(bReq.ReserveDataItems);

                foreach (var reqData in bBoxReq)
                {
                    string qStyle = reqData.QStyle;
                    string deliveryDate = reqData.DeliveryDate;
                    string insArticleList = reqData.InsArticleList;
                    string timeType = reqData.TimeType;
                    string timeNo = reqData.TimeNo;


                    if (reqData.DataItems.Any())
                    {
                        foreach (var dataItems in reqData.DataItems)
                        {
                            string artNo = dataItems.ArtNo;
                            //200000--->Normal
                            //7002072--->Sameday
                            //7002131-- > Nextday
                            if (!new[] { "200000", "7002072", "7002131" }.Contains(artNo))
                            {
                                List<TimeGroupItem> timeGroupItem = new List<TimeGroupItem>();
                                List<ResponseReserveDataItem> aBoxResp = new List<ResponseReserveDataItem>();

                                if (aResp.InquiryRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquiryRs.ReserveDataItems);
                                if (aResp.InquirySameDayRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                                if (aResp.InquiryNextDayRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);

                                #region aBoxResp
                                if (aBoxResp?.Any() == true)
                                {
                                    bool foundArtNo = false, foundTime = false;

                                    foreach (var box in aBoxResp)
                                    {
                                        if (box?.DataItems?.Any(d => d?.ArtNo == artNo) == true)
                                        {
                                            foundArtNo = true;

                                            if (qStyle == "N")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                                t?.TimeGrpNo == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.TimeGrpQty == 0) == true
                                                            ||
                                                            box.ReadyReserve?.Befores?.Any(t =>
                                                                t?.Date == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.Qty == 0) == true
                                                            ||
                                                            box.ReadyReserve?.Afters?.Any(t =>
                                                                t?.Date == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.Qty == 0) == true;
                                            }
                                            else if (qStyle == "S" || qStyle == "X")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                                t?.TimeGrpNo == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.TimeGrpQty == 0) == true;
                                            }
                                        }

                                        if (foundTime)
                                        {
                                            break;
                                        }
                                    }
                                    if (foundArtNo && foundTime)
                                    {
                                       
                                        insufficientItems.Add($"QStyle:{qStyle} ArtNo:{artNo}");
                                        caseType.StatusCase = true;
                                        caseType.CaseTypeReviews = "Capa เป็น 0 หน้า web ปล่อยซื้อได้ ";
                                        string result = string.Join(Environment.NewLine, insufficientItems);
                                        caseType.RootCause = result;
                                        return await Task.FromResult(caseType);
                                    }
                                }
                                #endregion
                            }
                        }
                    }
                }

            }
            catch { }
            return await Task.FromResult(caseType);
        }

        //5. Capa ds  เป็น 0 ไม่สามารถจองคิวได้
        //ข้อ 5.Capa หมด ระหว่างจองคิว//O5957695//O5957273//O5957200
        public async Task<CaseType> QueueBlockedZeroCapaDS(JsonDSRequest bReq, JsonDSResponse aResp, JsonDSResponse cResp)
        {
            CaseType caseType = new CaseType();
            var insufficientItems = new List<string>();
            try
            {
                var bBoxReq = new List<ReserveDataItems>();
                if (bReq.ReserveDataItems != null)
                    bBoxReq.AddRange(bReq.ReserveDataItems);

                foreach (var reqData in bBoxReq)
                {
                    string qStyle = reqData.QStyle;
                    string deliveryDate = reqData.DeliveryDate;
                    string insArticleList = reqData.InsArticleList;
                    string timeType = reqData.TimeType;
                    string timeNo = reqData.TimeNo;


                    if (reqData.DataItems.Any())
                    {
                        foreach (var dataItems in reqData.DataItems)
                        {
                            string artNo = dataItems.ArtNo;
                            //200000--->Normal
                            //7002072--->Sameday
                            //7002131-- > Nextday
                            if (!new[] { "200000", "7002072", "7002131" }.Contains(artNo))
                            {
                                List<TimeGroupItem> timeGroupItem = new List<TimeGroupItem>();
                                List<ResponseReserveDataItem> aBoxResp = new List<ResponseReserveDataItem>();

                                if (aResp.InquiryRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquiryRs.ReserveDataItems);
                                if (aResp.InquirySameDayRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                                if (aResp.InquiryNextDayRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);

                                List<ResponseReserveDataItem> cBoxResp = new List<ResponseReserveDataItem>();
                                if (cResp.InquiryRs?.ReserveDataItems != null)
                                    cBoxResp.AddRange(cResp.InquiryRs.ReserveDataItems);
                                if (cResp.InquirySameDayRs?.ReserveDataItems != null)
                                    cBoxResp.AddRange(cResp.InquirySameDayRs.ReserveDataItems);
                                if (cResp.InquiryNextDayRs?.ReserveDataItems != null)
                                    cBoxResp.AddRange(cResp.InquiryNextDayRs.ReserveDataItems);

                                bool aBoxRes = false;
                                bool cBoxRes = false;

                                #region aBoxResp
                                if (aBoxResp?.Any() == true)
                                {
                                    bool foundArtNo = false, foundTime = false;

                                    foreach (var box in aBoxResp)
                                    {
                                        if (box?.DataItems?.Any(d => d?.ArtNo == artNo) == true)
                                        {
                                            foundArtNo = true;

                                            if (qStyle == "N")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                                t?.TimeGrpNo == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.TimeGrpQty > 0) == true
                                                            ||
                                                            box.ReadyReserve?.Befores?.Any(t =>
                                                                t?.Date == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.Qty > 0) == true
                                                            ||
                                                            box.ReadyReserve?.Afters?.Any(t =>
                                                                t?.Date == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.Qty > 0) == true;
                                            }
                                            else if (qStyle == "S" || qStyle == "X")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                                t?.TimeGrpNo == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.TimeGrpQty > 0) == true;
                                            }
                                        }

                                        if (foundTime)
                                        {
                                            break;
                                        }
                                    }

                                    if (foundArtNo && foundTime)
                                    {
                                        aBoxRes = true;
                                    }
                                }
                                #endregion

                                #region cBoxResp
                                if (cBoxResp?.Any() == true)
                                {
                                    bool foundArtNo = false, foundTime = false;

                                    foreach (var box in cBoxResp)
                                    {
                                        if (box?.DataItems?.Any(d => d?.ArtNo == artNo) == true)
                                        {
                                            foundArtNo = true;

                                            if (qStyle == "N")
                                            {
                                                foundTime =
                                                    box.ReadyReserveTimeGrp?.Any(t =>
                                                        t?.TimeGrpNo == deliveryDate &&
                                                        t.TimeNo == timeNo &&
                                                        t.TimeGrpQty == 0) == true
                                                    ||
                                                    box.ReadyReserve?.Befores?.Any(t =>
                                                        t?.Date == deliveryDate &&
                                                        t.TimeNo == timeNo &&
                                                        t.Qty == 0) == true
                                                    ||
                                                    box.ReadyReserve?.Afters?.Any(t =>
                                                        t?.Date == deliveryDate &&
                                                        t.TimeNo == timeNo &&
                                                        t.Qty == 0) == true;
                                            }
                                            else if (qStyle == "S" || qStyle == "X")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                    t?.TimeGrpNo == deliveryDate &&
                                                    t.TimeNo == timeNo &&
                                                    t.TimeGrpQty == 0) == true;
                                            }

                                        }
                                        if (foundTime)
                                        {
                                            break;
                                        }
                                    }

                                    if (foundArtNo && foundTime)
                                    {
                                        cBoxRes = true;
                                    }
                                }
                                #endregion

                                if (aBoxRes && cBoxRes)
                                {
                                   
                                    // insufficientItems.Add($"QStyle:{reqGroup.QStyle} ArtNo:{artNo} | Stock:{respStockQty} < Request:{reqQty}");
                                    caseType.StatusCase = true;
                                    caseType.CaseTypeReviews = "Capa ds เป็น 0 ระหว่างจองคิว";
                                    insufficientItems.Add($"QStyle:{qStyle} ArtNo:{artNo}");
                                    string result = string.Join(Environment.NewLine, insufficientItems);
                                    caseType.RootCause = result;
                                    return await Task.FromResult(caseType);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        //6. Capa ds  มี Stock  มี จองคิวไม่ได้
        //ข้อ 6 Capa มี Stock มี จองคิวไม่ได้//O5956814//O5940226//O5939229
        public async Task<CaseType> QueueBlockedDespiteStockCapaDS(JsonDSRequest bReq, JsonDSResponse aResp, JsonDSResponse cResp)
        {
            CaseType caseType = new CaseType();
            string _QStyle = "";
            string _artNo = "";
            var insufficientItems = new List<string>();
            try
            {
                var bBoxReq = new List<ReserveDataItems>();
                if (bReq.ReserveDataItems != null)
                    bBoxReq.AddRange(bReq.ReserveDataItems);

                foreach (var reqData in bBoxReq)
                {
                    string qStyle = reqData.QStyle;
                    string deliveryDate = reqData.DeliveryDate;
                    string insArticleList = reqData.InsArticleList;
                    string timeType = reqData.TimeType;
                    string timeNo = reqData.TimeNo;


                    if (reqData.DataItems.Any())
                    {
                        foreach (var dataItems in reqData.DataItems)
                        {
                            string artNo = dataItems.ArtNo;
                            //200000--->Normal
                            //7002072--->Sameday
                            //7002131-- > Nextday
                            if (!new[] { "200000", "7002072", "7002131" }.Contains(artNo))
                            {
                                List<TimeGroupItem> timeGroupItem = new List<TimeGroupItem>();
                                List<ResponseReserveDataItem> aBoxResp = new List<ResponseReserveDataItem>();

                                if (aResp.InquiryRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquiryRs.ReserveDataItems);
                                if (aResp.InquirySameDayRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                                if (aResp.InquiryNextDayRs?.ReserveDataItems != null)
                                    aBoxResp.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);

                                List<ResponseReserveDataItem> cBoxResp = new List<ResponseReserveDataItem>();
                                if (cResp.InquiryRs?.ReserveDataItems != null)
                                    cBoxResp.AddRange(cResp.InquiryRs.ReserveDataItems);
                                if (cResp.InquirySameDayRs?.ReserveDataItems != null)
                                    cBoxResp.AddRange(cResp.InquirySameDayRs.ReserveDataItems);
                                if (cResp.InquiryNextDayRs?.ReserveDataItems != null)
                                    cBoxResp.AddRange(cResp.InquiryNextDayRs.ReserveDataItems);

                                bool aBoxRes = false;
                                bool cBoxRes = false;

                                #region aBoxResp
                                if (aBoxResp?.Any() == true)
                                {
                                    bool foundArtNo = false, foundTime = false;

                                    foreach (var box in aBoxResp)
                                    {
                                        if (box?.DataItems?.Any(d => d?.ArtNo == artNo) == true)
                                        {
                                            foundArtNo = true;
                                            _artNo = artNo;
                                            if (qStyle == "N")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                                t?.TimeGrpNo == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.TimeGrpQty > 0) == true
                                                            ||
                                                            box.ReadyReserve?.Befores?.Any(t =>
                                                                t?.Date == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.Qty > 0) == true
                                                            ||
                                                            box.ReadyReserve?.Afters?.Any(t =>
                                                                t?.Date == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.Qty > 0) == true;
                                            }
                                            else if (qStyle == "S" || qStyle == "X")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                                t?.TimeGrpNo == deliveryDate &&
                                                                t.TimeNo == timeNo &&
                                                                t.TimeGrpQty > 0) == true;
                                            }
                                        }

                                        if (foundTime)
                                        {
                                            break;
                                        }
                                    }

                                    if (foundArtNo && foundTime)
                                    {
                                        aBoxRes = true;
                                    }
                                }
                                #endregion

                                #region cBoxResp
                                if (cBoxResp?.Any() == true)
                                {
                                    bool foundArtNo = false, foundTime = false;

                                    foreach (var box in cBoxResp)
                                    {
                                        if (box?.DataItems?.Any(d => d?.ArtNo == artNo) == true)
                                        {
                                            foundArtNo = true;
                                            _artNo = artNo;
                                            if (qStyle == "N")
                                            {
                                                foundTime =
                                                    box.ReadyReserveTimeGrp?.Any(t =>
                                                        t?.TimeGrpNo == deliveryDate &&
                                                        t.TimeNo == timeNo &&
                                                        t.TimeGrpQty > 0) == true
                                                    ||
                                                    box.ReadyReserve?.Befores?.Any(t =>
                                                        t?.Date == deliveryDate &&
                                                        t.TimeNo == timeNo &&
                                                        t.Qty > 0) == true
                                                    ||
                                                    box.ReadyReserve?.Afters?.Any(t =>
                                                        t?.Date == deliveryDate &&
                                                        t.TimeNo == timeNo &&
                                                        t.Qty > 0) == true;
                                            }
                                            else if (qStyle == "S" || qStyle == "X")
                                            {
                                                foundTime = box.ReadyReserveTimeGrp?.Any(t =>
                                                    t?.TimeGrpNo == deliveryDate &&
                                                    t.TimeNo == timeNo &&
                                                    t.TimeGrpQty > 0) == true;
                                            }

                                        }

                                        if (foundTime)
                                        {
                                            break;
                                        }
                                    }

                                    if (foundArtNo && foundTime)
                                    {
                                        cBoxRes = true;
                                    }
                                }
                                #endregion

                                if (aBoxRes && cBoxRes)
                                {
                                    insufficientItems.Add($"QStyle:{qStyle} ArtNo:{artNo}");
                                    string result = string.Join(Environment.NewLine, insufficientItems);
                                    caseType.RootCause = result;
                                    // insufficientItems.Add($"QStyle:{reqGroup.QStyle} ArtNo:{artNo} | Stock:{respStockQty} < Request:{reqQty}");
                                    caseType.StatusCase = true;
                                    caseType.CaseTypeReviews = "Capa ds มี Stock มี จองคิวไม่ได้";
                                    return await Task.FromResult(caseType);
                                }
                            }
                        }
                    }
                }
            }
            catch { }
            return await Task.FromResult(caseType);
        }
        private void Txt01_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void ExportExcel_Click(object sender, RoutedEventArgs e)
        {
            // 1. Get your data (replace with your actual data source)
            //List<Order> orders = GetSampleOrders();
            if (DgLoadExcel.ItemsSource is not IEnumerable<Order> orders)
            {
                MessageBox.Show("No data available to export", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Convert to list for counting and multiple enumeration
            var ordersList = orders.ToList();
            if (ordersList.Count == 0)
            {
                MessageBox.Show("No orders found to export", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }



            // 2. Create and configure the save file dialog
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = $"OrdersExport_{DateTime.Now:yyyyMMdd_HHmmss}",
                Title = "Save Orders Export"
            };

            // 3. Show the dialog and process the result
            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    // 4. Export the data
                    ExcelExporter  epEcel = new ExcelExporter();
                    epEcel.ExportOrdersToExcel(ordersList, saveFileDialog.FileName);

                    // 5. Show success message
                    var result = MessageBox.Show(
                        $"Successfully exported {ordersList.Count} orders to:\n{saveFileDialog.FileName}\n\nOpen file now?",
                        "Export Complete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                        {
                            FileName = saveFileDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting orders:\n{ex.Message}",
                                    "Export Failed",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                }
            }
        }
    }
}


