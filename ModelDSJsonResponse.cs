using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTestCase
{
    using System;
    using System.Collections.Generic;

    public class JsonDsResponse
    {
        public string ReserveStatus { get; set; }
        public string ErrorMsg { get; set; }
        public string FlagTransfee { get; set; }
        public InquiryRs InquiryRs { get; set; }
        public InquirySameDayRs InquirySameDayRs { get; set; }
        public InquiryNextDayRs InquiryNextDayRs { get; set; }
        public InquiryDeliveryNowRs InquiryDeliveryNowRs { get; set; }
        public List<object> Queues { get; set; }
        public MessageStatusRs MessageStatusRs { get; set; }
    }

    public class InquiryRs
    {
        public List<object> ArticleMinPurchaseDeliveryFees { get; set; }
        public List<ReserveDataItem> ReserveDataItems { get; set; }
    }

    public class InquirySameDayRs
    {
        public List<SameDayReserveDataItem> ReserveDataItems { get; set; }
    }

    public class InquiryNextDayRs
    {
        public List<NextDayReserveDataItem> ReserveDataItems { get; set; }
    }

    public class InquiryDeliveryNowRs
    {
        public List<object> ReserveDataItems { get; set; }
    }

    public class MessageStatusRs
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class ReserveDataItem
    {
        public string Pattype { get; set; }
        public string QStyle { get; set; }
        public List<object> ArticleDeliveryFees { get; set; }
        public List<DataItem> DataItems { get; set; }
        public object AvailableQty { get; set; }
        public List<TimeGrp> ReadyReserveTimeGrp { get; set; }
        public ReadyReserve ReadyReserve { get; set; }
        public string ReserveMsg { get; set; }
        public string ReserveStatus { get; set; }
        public object ReadyDate { get; set; }
        public string IsOnOrder { get; set; }
        public double MinimumOrder { get; set; }
        public object AreaId { get; set; }
        public object Ref_Article { get; set; }
    }

    public class SameDayReserveDataItem
    {
        public string ShippointManage { get; set; }
        public string JobNo { get; set; }
        public string JobType { get; set; }
        public string PrdNo { get; set; }
        public object VendorGroupNo { get; set; }
        public List<ArticleDeliveryFee> ArticleDeliveryFees { get; set; }
        public List<SameDayDataItem> DataItems { get; set; }
        public object AvailableQty { get; set; }
        public List<SameDayTimeGrp> ReadyReserveTimeGrp { get; set; }
        public string ReserveMsg { get; set; }
        public string ReserveStatus { get; set; }
        public object ReadyDate { get; set; }
        public string Pattype { get; set; }
        public string QStyle { get; set; }
    }

    public class NextDayReserveDataItem
    {
        public string ShippointManage { get; set; }
        public string JobNo { get; set; }
        public string JobType { get; set; }
        public string PrdNo { get; set; }
        public object VendorGroupNo { get; set; }
        public List<ArticleDeliveryFee> ArticleDeliveryFees { get; set; }
        public List<NextDayDataItem> DataItems { get; set; }
        public object AvailableQty { get; set; }
        public List<NextDayTimeGrp> ReadyReserveTimeGrp { get; set; }
        public string ReserveMsg { get; set; }
        public string ReserveStatus { get; set; }
        public object ReadyDate { get; set; }
        public string Pattype { get; set; }
        public string QStyle { get; set; }
    }

    public class DataItem
    {
        public string Shippoint { get; set; }
        public string LineItem { get; set; }
        public string ArtNo { get; set; }
        public object CBNo { get; set; }
        public string DeliverySite { get; set; }
        public object VendorId { get; set; }
        public double? StockQty { get; set; }
        public double Qty { get; set; }
    }

    public class SameDayDataItem
    {
        public string Shippoint { get; set; }
        public string LineItem { get; set; }
        public string ArtNo { get; set; }
        public object CBNo { get; set; }
        public string DeliverySite { get; set; }
        public object QArtkey { get; set; }
        public object ItemText { get; set; }
        public object VendorId { get; set; }
        public double? StockQty { get; set; }
        public double Qty { get; set; }
    }

    public class NextDayDataItem
    {
        public string Shippoint { get; set; }
        public string LineItem { get; set; }
        public string ArtNo { get; set; }
        public object CBNo { get; set; }
        public string DeliverySite { get; set; }
        public object QArtkey { get; set; }
        public object ItemText { get; set; }
        public object VendorId { get; set; }
        public double? StockQty { get; set; }
        public double Qty { get; set; }
    }

    public class TimeGrp
    {
        public string TimeGrpNo { get; set; }
        public string TimeNo { get; set; }
        public string TimeName { get; set; }
        public int TimeGrpQty { get; set; }
        public object WorkerAvailable { get; set; }
    }

    public class SameDayTimeGrp
    {
        public string TimeGrpNo { get; set; }
        public string TimeNo { get; set; }
        public string TimeName { get; set; }
        public int TimeGrpQty { get; set; }
    }

    public class NextDayTimeGrp
    {
        public string TimeGrpNo { get; set; }
        public string TimeNo { get; set; }
        public string TimeName { get; set; }
        public int TimeGrpQty { get; set; }
    }

    public class ReadyReserve
    {
        public List<TimeSlot> Befores { get; set; }
        public List<TimeSlot> Afters { get; set; }
    }

    public class TimeSlot
    {
        public string Date { get; set; }
        public string TimeNo { get; set; }
        public string TimeName { get; set; }
        public double Qty { get; set; }
        public object WorkerAvailable { get; set; }
    }

    public class ArticleDeliveryFee
    {
        public string ArtNo { get; set; }
        public string ArtDesc { get; set; }
        public double TotalPrice { get; set; }
        public string Unit { get; set; }
        public string ItemUpc { get; set; }
        public string FlagCLM { get; set; }
        public string DeliFeeType { get; set; }
        public string VendorNo { get; set; }
        public string CondDesc { get; set; }
    }
}
