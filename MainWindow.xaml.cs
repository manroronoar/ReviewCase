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
using DocumentFormat.OpenXml.Wordprocessing;
using DocumentFormat.OpenXml.Office2016.Excel;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Drawing;
using System.Linq;

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

                    var processData = await ProcessEventsStatus(rawDataExcel.ToList(), progress);

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
        public async Task<PatternTbEvents> SequenceEventsStructureOne(List<TbEvents> events)
        {
            PatternTbEvents pattern = new PatternTbEvents();
            List<TbEvents> lis = new List<TbEvents>();
            //int i = 1;

            var e = events;
            if (e.Any())
            {
                var count = e.Count;

                //Pattern seq 1
                if (e[0].Type == "ORDER" && e[1].Type == "QUEUE" && e[2].Type == "QUEUE" && e[3].Type == "QUEUE" && e[4].Type == "CUSTOMER" && e[5].Type == "QUEUE")
                {
                    pattern.SeqPattern = 1;
                    pattern.SeqCount = 6;
                    pattern.boxA = 0;
                    pattern.boxB = 2;
                    pattern.boxC = 3;
                    pattern.boxD = 4;
                    pattern.boxE = 5;
                }
                //Pattern seq 2 todo




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
            int i = 1;
            foreach (var e in order)
            {
                if (!string.IsNullOrEmpty(e.OrderId))
                {

                    List<TbEvents> lstEvents = await ConnectionDB(e.OrderId);
                    var dataSeqEvents = await SequenceEventsStructureOne(lstEvents);
                    var dataSeq = dataSeqEvents.tbEvents.OrderBy(x => x.Seq).ToList();
                    if (dataSeq.Any())
                    {
                        // หาแพตเทอว่าอยู่ในรูปแบบ box แบบไหนก่อน

                        CaseType caseType = new CaseType();
                        #region 1. Server DS Down ชั่วคราว
                        caseType = await TempServerDSDown(lstEvents);
                        if (caseType != null)
                        {
                            e.CaseReviews = caseType.CaseTypeReviews;
                        }
                        #endregion

                        #region 2.logic Stock หน้า web คำนวนผิด
                        if (caseType != null)
                        {
                            JsonDSRequest? bReq = JsonConvert.DeserializeObject<JsonDSRequest>(dataSeq[dataSeqEvents.boxB].Req) ?? null;
                            JsonDSResponse? aResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxA].Resp) ?? null;
                            caseType = await WebStockLogicError(bReq, aResp);
                            e.CaseReviews = caseType.CaseTypeReviews;
                        }
                        #endregion

                        #region 3. Stock ds หมดระหว่างจองคิว
                        if (caseType != null) continue;
                        {
                            JsonDSRequest? bReq = JsonConvert.DeserializeObject<JsonDSRequest>(dataSeq[dataSeqEvents.boxB].Req) ?? null;
                            JsonDSResponse? aResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxA].Resp) ?? null;
                            JsonDSResponse? cResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxC].Resp) ?? null;
                            caseType = await StockDSOutDuringQueue(bReq, aResp, cResp);
                            e.CaseReviews = caseType.CaseTypeReviews;
                        }
                        #endregion

                        #region 4. Capa เป็น 0 หน้า web ปล่อยซื้อได้
                        if (caseType != null)
                        {
                            JsonDSRequest? bReq = JsonConvert.DeserializeObject<JsonDSRequest>(dataSeq[dataSeqEvents.boxB].Req) ?? null;
                            JsonDSResponse? aResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxA].Resp) ?? null;
                            caseType = await WebPurchaseAllowedZeroCapa(bReq, aResp);
                            e.CaseReviews = caseType.CaseTypeReviews;
                        }
                        #endregion

                        #region 5. Capa ds  เป็น 0 ไม่สามารถจองคิวได้
                        if (caseType != null) continue;
                        {
                            JsonDSRequest? bReq = JsonConvert.DeserializeObject<JsonDSRequest>(dataSeq[dataSeqEvents.boxB].Req) ?? null;
                            JsonDSResponse? aResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxA].Resp) ?? null;
                            JsonDSResponse? cResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxC].Resp) ?? null;
                            caseType = await QueueBlockedZeroCapaDS(bReq, aResp, cResp);
                            e.CaseReviews = caseType.CaseTypeReviews;
                        }
                        #endregion

                        #region 6. Capa ds  มี Stock  มี จองคิวไม่ได้
                        if (caseType != null) continue;
                        {
                            JsonDSRequest? bReq = JsonConvert.DeserializeObject<JsonDSRequest>(dataSeq[dataSeqEvents.boxB].Req) ?? null;
                            JsonDSResponse? aResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxA].Resp) ?? null;
                            JsonDSResponse? cResp = JsonConvert.DeserializeObject<JsonDSResponse>(dataSeq[dataSeqEvents.boxC].Resp) ?? null;
                            caseType = await QueueBlockedDespiteStockCapaDS(bReq, aResp, cResp);
                            e.CaseReviews = caseType.CaseTypeReviews;
                        }
                        #endregion

                    }
                    else
                    {
                        CaseType caseType = new CaseType();
                        #region 1. Server DS Down ชั่วคราว
                        caseType = await TempServerDSDown(lstEvents);
                        e.CaseReviews = caseType.CaseTypeReviews;
                        #endregion

                        if (caseType == null)
                        {
                            e.CaseReviews = "Ignore Pattern Flow";
                        }

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
                }
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        //2. logic Stock หน้า web คำนวนผิด
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
                if (aResp.InquiryDeliveryNowRs?.ReserveDataItems != null)
                    allResponseItems.AddRange(aResp.InquiryDeliveryNowRs.ReserveDataItems);

                // Loop ใน request
                foreach (var reqGroup in bReq.ReserveDataItems)
                {
                    if (!new[] { "N", "S", "X" }.Contains(reqGroup.QStyle)) continue;

                    foreach (var reqItem in reqGroup.DataItems)
                    {
                        var artNo = reqItem.ArtNo;
                        var reqQty = double.TryParse(reqItem.Qty, out var parsedQty) ? parsedQty : 0;

                        // หา response item ที่ ArtNo ตรงกัน
                        var matchingRespItem = allResponseItems
                            .Where(r => r.QStyle == reqGroup.QStyle)
                            .SelectMany(r => r.DataItems)
                            .FirstOrDefault(d => d.ArtNo == artNo);

                        if (matchingRespItem != null)
                        {
                            double respStockQty = 0;
                            if (matchingRespItem.StockQty is string sqStr)
                                double.TryParse(sqStr, out respStockQty);
                            else if (matchingRespItem.StockQty is double sqDouble)
                                respStockQty = sqDouble;
                            else if (matchingRespItem.StockQty is int sqInt)
                                respStockQty = sqInt;

                            if (respStockQty < reqQty)
                            {
                                insufficientItems.Add($"QStyle:{reqGroup.QStyle} ArtNo:{artNo} | Stock:{respStockQty} < Request:{reqQty}");
                            }
                        }
                    }
                }


                if (insufficientItems.Any())
                {
                    caseType.StatusCase = true;
                    caseType.CaseTypeReviews = "logic Stock หน้า web คำนวนผิด";
                }
                return caseType;
            }
            catch
            {

            }
            return await Task.FromResult(caseType);
        }

        //3. Stock ds หมดระหว่างจองคิว
        public async Task<CaseType> StockDSOutDuringQueue(JsonDSRequest bReq, JsonDSResponse aResp, JsonDSResponse cResp)
        {
            CaseType caseType = new CaseType();
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
                    if (aResp.InquiryDeliveryNowRs?.ReserveDataItems != null)
                        aBoxResp.AddRange(aResp.InquiryDeliveryNowRs.ReserveDataItems);


                    var cBoxResp = new List<ResponseReserveDataItem>();
                    if (cResp.InquiryRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquiryRs.ReserveDataItems);
                    if (cResp.InquirySameDayRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquirySameDayRs.ReserveDataItems);
                    if (cResp.InquiryNextDayRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquiryNextDayRs.ReserveDataItems);
                    if (cResp.InquiryDeliveryNowRs?.ReserveDataItems != null)
                        cBoxResp.AddRange(cResp.InquiryDeliveryNowRs.ReserveDataItems);


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
                                caseType.StatusCase = true;
                                caseType.CaseTypeReviews = "Stock ds หมดระหว่างจองคิว";
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
        public async Task<CaseType> WebPurchaseAllowedZeroCapa(JsonDSRequest bReq, JsonDSResponse aResp)
        {
            CaseType caseType = new CaseType();
            try
            {
                var bBoxReq = new List<ReserveDataItems>();
                if (bReq.ReserveDataItems != null)
                    bBoxReq.AddRange(bBoxReq);

                foreach (var reqData in bBoxReq)
                {
                    string qStyle = reqData.QStyle;
                    string deliveryDate = reqData.DeliveryDate;
                    string insArticleList = reqData.InsArticleList;
                    string timeType = reqData.TimeType;
                    string timeNo = reqData.TimeNo;


                    if (reqData.DataItems.Any()) continue;
                    foreach (var dataItems in reqData.DataItems)
                    {
                        string artNo = dataItems.ArtNo;
                        //200000--->Normal
                        //7002072--->Sameday
                        //7002131-- > Nextday
                        if (!new[] { "200000", "7002072", "7002131" }.Contains(artNo)) continue;
                        List<TimeGroupItem> timeGroupItem = new List<TimeGroupItem>();
                        List<ResponseReserveDataItem> aBoxResp = new List<ResponseReserveDataItem>();

                        if (aResp.InquiryRs?.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquiryRs.ReserveDataItems);
                        if (aResp.InquirySameDayRs.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                        if (aResp.InquiryNextDayRs.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);

                        if (aBoxResp.Any())
                        {
                            //check ArtNo Time
                            bool foundArtNo = false;
                            bool foundTime = false;
                            foundArtNo = aBoxResp.Any(m => m.DataItems.Any(q => q.ArtNo == artNo));

                            if (foundArtNo)
                            {
                                if (qStyle == "N")
                                {

                                    foundTime = aBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty == 0
                                                                                               && q.TimeGrpNo == timeNo));
                                    if (!foundTime)
                                    {
                                        foundTime = aBoxResp.Any(m => m.ReadyReserve.Befores.Any(q => q.Date == deliveryDate
                                                                                                   && q.Qty == 0
                                                                                                   && q.TimeNo == timeNo));
                                    }

                                    if (!foundTime)
                                    {
                                        foundTime = aBoxResp.Any(m => m.ReadyReserve.Afters.Any(q => q.Date == deliveryDate
                                                                                               && q.Qty == 0
                                                                                               && q.TimeNo == timeNo));
                                    }

                                }
                                else if (qStyle == "S" || qStyle == "X")
                                {
                                    foundTime = aBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty == 0
                                                                                               && q.TimeGrpNo == timeNo));
                                }

                            }

                            if (foundArtNo && foundTime)
                            {
                                caseType.StatusCase = true;
                                caseType.CaseTypeReviews = "Capa เป็น 0 หน้า web ปล่อยซื้อได้ ";
                                return await Task.FromResult(caseType);
                            }
                        }

                    }
                }

            }
            catch { }
            return await Task.FromResult(caseType);
        }

        //5. Capa ds  เป็น 0 ไม่สามารถจองคิวได้
        public async Task<CaseType> QueueBlockedZeroCapaDS(JsonDSRequest bReq, JsonDSResponse aResp, JsonDSResponse cResp)
        {
            CaseType caseType = new CaseType();
            try
            {
                var bBoxReq = new List<ReserveDataItems>();
                if (bReq.ReserveDataItems != null)
                    bBoxReq.AddRange(bBoxReq);

                foreach (var reqData in bBoxReq)
                {
                    string qStyle = reqData.QStyle;
                    string deliveryDate = reqData.DeliveryDate;
                    string insArticleList = reqData.InsArticleList;
                    string timeType = reqData.TimeType;
                    string timeNo = reqData.TimeNo;


                    if (reqData.DataItems.Any()) continue;
                    foreach (var dataItems in reqData.DataItems)
                    {
                        string artNo = dataItems.ArtNo;
                        //200000--->Normal
                        //7002072--->Sameday
                        //7002131-- > Nextday
                        if (!new[] { "200000", "7002072", "7002131" }.Contains(artNo)) continue;
                        List<TimeGroupItem> timeGroupItem = new List<TimeGroupItem>();
                        List<ResponseReserveDataItem> aBoxResp = new List<ResponseReserveDataItem>();

                        if (aResp.InquiryRs?.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquiryRs.ReserveDataItems);
                        if (aResp.InquirySameDayRs.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                        if (aResp.InquiryNextDayRs.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);

                        List<ResponseReserveDataItem> cBoxResp = new List<ResponseReserveDataItem>();
                        if (cResp.InquiryRs?.ReserveDataItems != null)
                            cBoxResp.AddRange(cResp.InquiryRs.ReserveDataItems);
                        if (cResp.InquirySameDayRs.ReserveDataItems != null)
                            cBoxResp.AddRange(cResp.InquirySameDayRs.ReserveDataItems);
                        if (cResp.InquiryNextDayRs.ReserveDataItems != null)
                            cBoxResp.AddRange(cResp.InquiryNextDayRs.ReserveDataItems);

                        bool aBoxRes = false;
                        bool cBoxRes = false;

                        #region aBoxResp
                        if (aBoxResp.Any())
                        {
                            //check ArtNo Time
                            bool foundArtNo = false;
                            bool foundTime = false;
                            foundArtNo = aBoxResp.Any(m => m.DataItems.Any(q => q.ArtNo == artNo));

                            if (foundArtNo)
                            {
                                if (qStyle == "N")
                                {

                                    foundTime = aBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty > 0
                                                                                               && q.TimeGrpNo == timeNo));
                                    if (!foundTime)
                                    {
                                        foundTime = aBoxResp.Any(m => m.ReadyReserve.Befores.Any(q => q.Date == deliveryDate
                                                                                                   && q.Qty > 0
                                                                                                   && q.TimeNo == timeNo));
                                    }

                                    if (!foundTime)
                                    {
                                        foundTime = aBoxResp.Any(m => m.ReadyReserve.Afters.Any(q => q.Date == deliveryDate
                                                                                               && q.Qty > 0
                                                                                               && q.TimeNo == timeNo));
                                    }

                                }
                                else if (qStyle == "S" || qStyle == "X")
                                {
                                    foundTime = aBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty == 0
                                                                                               && q.TimeGrpNo == timeNo));
                                }

                            }

                            if (foundArtNo && foundTime)
                            {
                                aBoxRes = true;
                            }
                        }
                        #endregion

                        #region cBoxResp
                        if (cBoxResp.Any())
                        {
                            //check ArtNo Time
                            bool foundArtNo = false;
                            bool foundTime = false;
                            foundArtNo = cBoxResp.Any(m => m.DataItems.Any(q => q.ArtNo == artNo));

                            if (foundArtNo)
                            {
                                if (qStyle == "N")
                                {

                                    foundTime = cBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty == 0
                                                                                               && q.TimeGrpNo == timeNo));
                                    if (!foundTime)
                                    {
                                        foundTime = cBoxResp.Any(m => m.ReadyReserve.Befores.Any(q => q.Date == deliveryDate
                                                                                                   && q.Qty == 0
                                                                                                   && q.TimeNo == timeNo));
                                    }

                                    if (!foundTime)
                                    {
                                        foundTime = cBoxResp.Any(m => m.ReadyReserve.Afters.Any(q => q.Date == deliveryDate
                                                                                               && q.Qty == 0
                                                                                               && q.TimeNo == timeNo));
                                    }

                                }
                                else if (qStyle == "S" || qStyle == "X")
                                {
                                    foundTime = cBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty == 0
                                                                                               && q.TimeGrpNo == timeNo));
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
                            caseType.StatusCase = true;
                            caseType.CaseTypeReviews = "Capa ds  เป็น 0 ระหว่างจองคิว";
                            return await Task.FromResult(caseType);
                        }

                    }


                }
            }
            catch { }
            return await Task.FromResult(caseType);
        }

        //6. Capa ds  มี Stock  มี จองคิวไม่ได้
        public async Task<CaseType> QueueBlockedDespiteStockCapaDS(JsonDSRequest bReq, JsonDSResponse aResp, JsonDSResponse cResp)
        {
            CaseType caseType = new CaseType();
            try
            {
                var bBoxReq = new List<ReserveDataItems>();
                if (bReq.ReserveDataItems != null)
                    bBoxReq.AddRange(bBoxReq);

                foreach (var reqData in bBoxReq)
                {
                    string qStyle = reqData.QStyle;
                    string deliveryDate = reqData.DeliveryDate;
                    string insArticleList = reqData.InsArticleList;
                    string timeType = reqData.TimeType;
                    string timeNo = reqData.TimeNo;


                    if (reqData.DataItems.Any()) continue;
                    foreach (var dataItems in reqData.DataItems)
                    {
                        string artNo = dataItems.ArtNo;
                        //200000--->Normal
                        //7002072--->Sameday
                        //7002131-- > Nextday
                        if (!new[] { "200000", "7002072", "7002131" }.Contains(artNo)) continue;
                        List<TimeGroupItem> timeGroupItem = new List<TimeGroupItem>();
                        List<ResponseReserveDataItem> aBoxResp = new List<ResponseReserveDataItem>();

                        if (aResp.InquiryRs?.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquiryRs.ReserveDataItems);
                        if (aResp.InquirySameDayRs.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquirySameDayRs.ReserveDataItems);
                        if (aResp.InquiryNextDayRs.ReserveDataItems != null)
                            aBoxResp.AddRange(aResp.InquiryNextDayRs.ReserveDataItems);

                        List<ResponseReserveDataItem> cBoxResp = new List<ResponseReserveDataItem>();
                        if (cResp.InquiryRs?.ReserveDataItems != null)
                            cBoxResp.AddRange(cResp.InquiryRs.ReserveDataItems);
                        if (cResp.InquirySameDayRs.ReserveDataItems != null)
                            cBoxResp.AddRange(cResp.InquirySameDayRs.ReserveDataItems);
                        if (cResp.InquiryNextDayRs.ReserveDataItems != null)
                            cBoxResp.AddRange(cResp.InquiryNextDayRs.ReserveDataItems);

                        bool aBoxRes = false;
                        bool cBoxRes = false;

                        #region aBoxResp
                        if (aBoxResp.Any())
                        {
                            //check ArtNo Time
                            bool foundArtNo = false;
                            bool foundTime = false;
                            foundArtNo = aBoxResp.Any(m => m.DataItems.Any(q => q.ArtNo == artNo));

                            if (foundArtNo)
                            {
                                if (qStyle == "N")
                                {

                                    foundTime = aBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty > 0
                                                                                               && q.TimeGrpNo == timeNo));
                                    if (!foundTime)
                                    {
                                        foundTime = aBoxResp.Any(m => m.ReadyReserve.Befores.Any(q => q.Date == deliveryDate
                                                                                                   && q.Qty > 0
                                                                                                   && q.TimeNo == timeNo));
                                    }

                                    if (!foundTime)
                                    {
                                        foundTime = aBoxResp.Any(m => m.ReadyReserve.Afters.Any(q => q.Date == deliveryDate
                                                                                               && q.Qty > 0
                                                                                               && q.TimeNo == timeNo));
                                    }

                                }
                                else if (qStyle == "S" || qStyle == "X")
                                {
                                    foundTime = aBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty > 0
                                                                                               && q.TimeGrpNo == timeNo));
                                }

                            }

                            if (foundArtNo && foundTime)
                            {
                                aBoxRes = true;
                            }
                        }
                        #endregion

                        #region cBoxResp
                        if (cBoxResp.Any())
                        {
                            //check ArtNo Time
                            bool foundArtNo = false;
                            bool foundTime = false;
                            foundArtNo = cBoxResp.Any(m => m.DataItems.Any(q => q.ArtNo == artNo));

                            if (foundArtNo)
                            {
                                if (qStyle == "N")
                                {

                                    foundTime = cBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty > 0
                                                                                               && q.TimeGrpNo == timeNo));
                                    if (!foundTime)
                                    {
                                        foundTime = cBoxResp.Any(m => m.ReadyReserve.Befores.Any(q => q.Date == deliveryDate
                                                                                                   && q.Qty > 0
                                                                                                   && q.TimeNo == timeNo));
                                    }

                                    if (!foundTime)
                                    {
                                        foundTime = cBoxResp.Any(m => m.ReadyReserve.Afters.Any(q => q.Date == deliveryDate
                                                                                               && q.Qty > 0
                                                                                               && q.TimeNo == timeNo));
                                    }

                                }
                                else if (qStyle == "S" || qStyle == "X")
                                {
                                    foundTime = cBoxResp.Any(m => m.ReadyReserveTimeGrp.Any(q => q.TimeGrpNo == deliveryDate
                                                                                               && q.TimeGrpQty > 0
                                                                                               && q.TimeGrpNo == timeNo));
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
                            caseType.StatusCase = true;
                            caseType.CaseTypeReviews = "Capa ds  มี Stock  มี จองคิวไม่ได้";
                            return await Task.FromResult(caseType);
                        }

                    }
                }
            }
            catch { }
            return await Task.FromResult(caseType);
        }

    }

}