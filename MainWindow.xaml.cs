using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using static WpfTestCase.MainWindow;
using System.Data;
using Npgsql;
using Dapper;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DocumentFormat.OpenXml.Office2013.Excel;
using DocumentFormat.OpenXml.Drawing.Charts;

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

            //O5629717


            //ProcessEvents(test, "sss");
            //DgLoadExcel.Visibility = Visibility.Collapsed;
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
                if (!string.IsNullOrEmpty(this.Txt01.Text.ToString()))
                {

                    List<Order> lisOrder = new List<Order>();
                    Order order = new Order();
                    order.OrderId = Txt01.Text;
                    lisOrder.Add(order);

                    // สร้าง Progress Reporter
                    var progress = new Progress<int>(percent =>
                    {
                        ProgressBar.Value = percent;
                        ProgressText.Text = $"{percent}%";
                    });

                    var processData = await ProcessEventsStatus(lisOrder.ToList(), progress);

                    // ต้องการ progress bar wpf c#

                    DgLoadExcel.ItemsSource = processData;
                    DgLoadExcel.Visibility = Visibility.Visible;
                }
                else 
                {
                    MessageBox.Show($"Input value", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
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

                    var processData = await ProcessEventsStatus(rawDataExcel.ToList(),progress);

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

                        orders.Add(new Order
                        {
                            RunNo = GetCellValue(cells[0], workbookPart),
                            Number = GetCellValue(cells[1], workbookPart),
                            MM = GetCellValue(cells[2], workbookPart),
                            TransactionDate = DateTime.Parse(GetCellValue(cells[3], workbookPart)),
                            OrderId = "O" + GetCellValue(cells[4], workbookPart),
                            TicketNo = GetCellValue(cells[5], workbookPart),
                            IsSameDay = GetCellValue(cells[6], workbookPart),
                            Delivery = GetCellValue(cells[7], workbookPart),
                            Status = GetCellValue(cells[8], workbookPart),
                            OrderError = GetCellValue(cells[9], workbookPart),
                            Pos = GetCellValue(cells[10], workbookPart),
                            RootCause = GetCellValue(cells[11], workbookPart),
                            Error = GetCellValue(cells[12], workbookPart),
                            Job = GetCellValue(cells[13], workbookPart),
                            CaseIrNo = GetCellValue(cells[14], workbookPart),
                            User = GetCellValue(cells[15], workbookPart),
                            CaseReviews = "Test"
                        });
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
                        db.Open();
                        var parameters = new DynamicParameters();
                        parameters.Add("Value", orderNo);
                        //parameters.Add("Limit", 10);
                        //select * from  events e where e.value = 'O5827423'
                        //lstEvents = db.Query<TbEvents>("SELECT * FROM events limit 10").ToList();
                        lstEvents = (List<TbEvents>)await db.QueryAsync<TbEvents>("SELECT * FROM events WHERE value = @Value and type in ('QUEUE','CUSTOMER','ORDER') order by createdate  desc", parameters);
                    }
                    catch (Exception ex)
                    {

                    }

                }
            }
            catch
            {

            }
            return await Task.FromResult(lstEvents);
        }
        public List<TbEvents> SequenceEventsStructureOne(List<TbEvents> events)
        {
            List<TbEvents> lis = new List<TbEvents>();
            List<TbEvents> lisEvent = new List<TbEvents>();
            int i = 1;

            foreach (TbEvents e in events)
            {
                if (i <= 6)
                {
                    if (i == 1 && e.Type == "ORDER")
                    {
                        e.Seq = 6;
                        lis.Add(e);
                    }
                    else if (i == 2 && e.Type == "QUEUE")
                    {
                        e.Seq = 5;
                        lis.Add(e);
                    }
                    else if (i == 3 && e.Type == "QUEUE")
                    {
                        e.Seq = 4;
                        lis.Add(e);
                    }
                    else if (i == 4 && e.Type == "QUEUE")
                    {
                        e.Seq = 3;
                        lis.Add(e);
                    }
                    else if (i == 5 && e.Type == "CUSTOMER")
                    {
                        e.Seq = 2;
                        lis.Add(e);
                    }
                    else if (i == 6 && e.Type == "QUEUE")
                    {
                        e.Seq = 1;
                        lis.Add(e);
                        return lis;
                    }
                    else
                    {
                        return lis = new List<TbEvents>();
                    }
                    i++;
                }
                else
                {
                    return lis = new List<TbEvents>();
                }
            }

            return lis;
        }
        private async Task<List<Order>> ProcessEventsStatus(List<Order> order, IProgress<int> progress)
        {
            int i = 1;
            foreach (var e in order)
            {
                if (!string.IsNullOrEmpty(e.OrderId))
                {

                    List<TbEvents> lstEvents = await ConnectionDB(e.OrderId);
                    var dataSeq = SequenceEventsStructureOne(lstEvents).OrderBy(n => n.Seq).ToList();
                    if (dataSeq.Any())
                    {
                        e.CaseReviews = "logic Case 1 2 3 4 5 6";

                        // หาแพตเทอว่าอยู่ในรูปแบบ box แบบไหนก่อน
                        //

                        #region 1. Server DS Down ชั่วคราว
                        var sss = TempServerDSDown(lstEvents);
                        #endregion

                        #region 2.logic Stock หน้า web คำนวนผิด
                        #endregion

                        #region 3. Stock ds หมดระหว่างจองคิว
                        #endregion

                        #region 4. Capa เป็น 0 หน้า web ปล่อยซื้อได้
                        #endregion

                        #region 5. Capa ds  เป็น 0 ไม่สามารถจองคิวได้
                        #endregion

                        #region 6. Capa ds  มี Stock  มี จองคิวไม่ได้
                        #endregion

                        #region logic Stock หน้า web คำนวนผิด && logic Stock ds หมดระหว่างจองคิว

                        JsonDsResponse? responseBox0 = JsonConvert.DeserializeObject<JsonDsResponse>(dataSeq[0].Resp) ?? null;
                        JsonDsResponse? responseBox2 = JsonConvert.DeserializeObject<JsonDsResponse>(dataSeq[2].Resp) ?? null;

                        CaseType caseType = await LogicStock(responseBox0, responseBox2, e.IsSameDay ?? "");
                        if (caseType.CaseTypeReviews != "")
                        {
                            e.CaseReviews = caseType.CaseTypeReviews;
                        }
                        #endregion

                    }
                    else
                    {
                        e.CaseReviews = "Ignore Block Flow";
                    }
                }
                // อัปเดต Progress (คำนวณเปอร์เซ็นต์)
                int percentComplete = (int)((i / (double)order.Count()) * 100);
                progress?.Report(percentComplete);

                // จำลองการหน่วงเวลา (ถ้าจำเป็น)
                //await Task.Delay(10);
                i++;
            }
            return await Task.FromResult(order);
        }


        //TempServerDSDown
        //WebStockLogicError
        //StockDSOutDuringQueue
        //WebPurchaseAllowedZeroCapa
        //QueueBlockedZeroCapaDS
        //QueueBlockedDespiteStockCapaDS


        public async Task<CaseType> TempServerDSDown(List<TbEvents> lstEvents)
        {
            CaseType caseType = new CaseType();
            try 
            {
                //1. Server DS Down ชั่วคราว
                List<TbEvents> res =  lstEvents.Where(e => e.HttpStatus.ToString() != "200").ToList();
                if (res.Any())
                {
                    caseType.StatusCase = true;
                    caseType.CaseTypeReviews = "Server DS Down ชั่วคราว";
                }
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        public async Task<CaseType> WebStockLogicError()
        {
            CaseType caseType = new CaseType();
            try 
            {
                //2. logic Stock หน้า web คำนวนผิด
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        public async Task<CaseType> StockDSOutDuringQueue()
        {
            CaseType caseType = new CaseType();
            try 
            {
                //3.Stock ds หมดระหว่างจองคิว
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        public async Task<CaseType> WebPurchaseAllowedZeroCapa()
        {
            CaseType caseType = new CaseType();
            try 
            {
                //4. Capa เป็น 0 หน้า web ปล่อยซื้อได้
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        public async Task<CaseType> QueueBlockedZeroCapaDS()
        {
            CaseType caseType = new CaseType();
            try 
            {
                //5. Capa ds  เป็น 0 ไม่สามารถจองคิวได้
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        public async Task<CaseType> QueueBlockedDespiteStockCapaDS()
        {
            CaseType caseType = new CaseType();
            try 
            {
                //6. Capa ds  มี Stock  มี จองคิวไม่ได้
            }
            catch { }
            return await Task.FromResult(caseType);
        }
        public async Task<CaseType> LogicStock(JsonDsResponse a, JsonDsResponse b,string sameday)
        {
            CaseType caseType = new CaseType();
            bool reaA = false;
            bool reaB = false;
            bool result = false;

            if (a != null && b != null)
            {

                if ((a.InquiryRs != null && b.InquiryRs != null && !result) && (sameday == "NORMAL" || sameday == ""))
                {
                    foreach (var item in a.InquiryRs.ReserveDataItems)
                    {
                        reaA = item.DataItems.Any(p => p.StockQty == 0);

                        if (reaA)
                        {
                            break;
                        }
                    }
                    foreach (var item in b.InquiryRs.ReserveDataItems)
                    {
                        reaB = item.DataItems.Any(p => p.StockQty == 0);

                        if (reaB)
                        {
                            break;
                        }
                    }
                    // A = 0 and B >= 0 =   logic Stock หน้า web คำนวนผิด
                    // A >= 0 and B = 0 =   Stock ds หมดระหว่างจองคิว

                    if (reaA && !reaB)
                    {
                        result = true;
                        caseType.CaseTypeReviews = "logic Stock หน้า web คำนวนผิด";
                        caseType.Starus = true;
                    }
                    else if (!reaA && reaB)
                    {
                        result = true;
                        caseType.CaseTypeReviews = "Stock ds หมดระหว่างจองคิว";
                        caseType.Starus = true;
                    }


                }
                else if (a.InquirySameDayRs != null && b.InquirySameDayRs != null && !result && (sameday == "SAMEDAY" || sameday == ""))
                {
                    foreach (var item in a.InquirySameDayRs.ReserveDataItems)
                    {
                        reaA = item.DataItems.Any(p => p.StockQty == 0);

                        if (reaA)
                        {
                            break;
                        }
                    }
                    foreach (var item in b.InquirySameDayRs.ReserveDataItems)
                    {
                        reaB = item.DataItems.Any(p => p.StockQty == 0);

                        if (reaB)
                        {
                            break;
                        }
                    }
                    if (reaA && !reaB)
                    {
                        result = true;
                        caseType.CaseTypeReviews = "logic Stock หน้า web คำนวนผิด";
                        caseType.Starus = true;
                    }
                    else if (!reaA && reaB)
                    {
                        result = true;
                        caseType.CaseTypeReviews = "Stock ds หมดระหว่างจองคิว";
                        caseType.Starus = true;
                    }
                }
                else if (a.InquiryNextDayRs != null && b.InquiryNextDayRs != null && !result && (sameday == "NEXTDAY" || sameday == ""))
                {
                    foreach (var item in a.InquiryNextDayRs.ReserveDataItems)
                    {
                        reaA = item.DataItems.Any(p => p.StockQty == 0);

                        if (reaA)
                        {
                            break;
                        }
                    }
                    foreach (var item in b.InquiryNextDayRs.ReserveDataItems)
                    {
                        reaB = item.DataItems.Any(p => p.StockQty == 0);

                        if (reaB)
                        {
                            break;
                        }
                    }

                    if (reaA && !reaB)
                    {
                        result = true;
                        caseType.CaseTypeReviews = ListNameCases.outOfStock;
                        caseType.Starus = true;
                    }
                    else if (!reaA && reaB)
                    {
                        result = true;
                        caseType.CaseTypeReviews = "Stock ds หมดระหว่างจองคิว";
                        caseType.Starus = true;
                    }
                }
               

            }
            return await Task.FromResult(caseType);
        }

       
    }

}