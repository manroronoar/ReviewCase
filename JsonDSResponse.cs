using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTestCase
{

    public class JsonDSResponse
    {
        public string ReserveStatus { get; set; }
        public string ErrorMsg { get; set; }
        public string FlagTransfee { get; set; }
        public InquiryRs InquiryRs { get; set; }
        public InquirySameDayRs InquirySameDayRs { get; set; }
        public InquiryNextDayRs InquiryNextDayRs { get; set; }
        public InquiryExpressRs InquiryExpressRs { get; set; }
        public List<QueueItem> Queues { get; set; }
        public MessageStatusRs MessageStatusRs { get; set; }
    }

    public class InquiryRs
    {
        public List<ArticleDeliveryFree> ArticleDeliveryFees { get; set; }
        public List<ResponseReserveDataItem> ReserveDataItems { get; set; }
    }

    public class InquirySameDayRs
    {
        public List<ArticleDeliveryFree> ArticleDeliveryFees { get; set; }
        public List<ResponseReserveDataItem> ReserveDataItems { get; set; }
    }

    public class InquiryNextDayRs
    {
        public List<ArticleDeliveryFree> ArticleDeliveryFees { get; set; }
        public List<ResponseReserveDataItem> ReserveDataItems { get; set; }
    }

    public class InquiryExpressRs
    {
        public List<ArticleDeliveryFree> ArticleDeliveryFees { get; set; }
        public List<ResponseReserveDataItem> ReserveDataItems { get; set; }
    }

    public class MessageStatusRs
    {
        public string Status { get; set; }
        public string Message { get; set; }
    }

    public class ResponseReserveDataItem
    {
        public string Pattype { get; set; }
        public string QStyle { get; set; }
        //public List<ArticleDeliveryFree> ArticleDeliveryFees { get; set; }
        public List<ResponseDataItem> DataItems { get; set; }
        public object AvailableQty { get; set; }
        public List<TimeGroupItem> ReadyReserveTimeGrp { get; set; }
        public ReadyReserve ReadyReserve { get; set; }
        public string ReserveMsg { get; set; }
        public string ReserveStatus { get; set; }
        public object ReadyDate { get; set; }
        public string IsOnOrder { get; set; }
        public double MinimumOrder { get; set; }
        public string AreaId { get; set; }
        public string Ref_Article { get; set; }
        public string ShippointManage { get; set; }
        public string JobNo { get; set; }
        public string JobType { get; set; }
        public string PrdNo { get; set; }
        public string VendorGroupNo { get; set; }
    }

    public class ArticleDeliveryFree
    {
        //public string ArtNo { get; set; }
        //public string ArtDesc { get; set; }
        //public double TotalPrice { get; set; }
        //public string Unit { get; set; }
        //public string ItemUpc { get; set; }
        //public string FlagCLM { get; set; }
        //public string DeliFeeType { get; set; }
        //public string VendorNo { get; set; }
        //public string CondDesc { get; set; }
        public string ArtNo { get; set; }
        public string ArtDesc { get; set; }
        public decimal TotalPrice { get; set; }
        public string Unit { get; set; }
        public string QStyle { get; set; }
        public string ServNo { get; set; }
        public string ItemUpc { get; set; }
        public string PatType { get; set; }
        public string FlagCLM { get; set; }
        public string VendorNo { get; set; }
        public string VendorName { get; set; }
        public string DeliFeeType { get; set; }
        public string DeliFeeTypeDesc { get; set; }
        public string CondDescTH { get; set; }
        public string CondDescEN { get; set; }
        public List<DeliFee> DeliFeeList { get; set; }
    }
    public class ResponseDataItem
    {
        public string Shippoint { get; set; }
        public string LineItem { get; set; }
        public string ArtNo { get; set; }
        public string CBNo { get; set; }
        public string DeliverySite { get; set; }
        public string VendorId { get; set; }
        public object StockQty { get; set; }
        public double Qty { get; set; }
        public string QArtkey { get; set; }
        public string ItemText { get; set; }
    }
    public class TimeGroupItem
    {
        public string TimeGrpNo { get; set; }
        public string TimeNo { get; set; }
        public string TimeName { get; set; }
        public double TimeGrpQty { get; set; }
        public object WorkerAvailable { get; set; }
    }
    public class ReadyReserve
    {
        public List<ReadyReserveItem> Befores { get; set; } = new List<ReadyReserveItem>();
        public List<ReadyReserveItem> Afters { get; set; } = new List<ReadyReserveItem>();
    }
    public class ReadyReserveItem
    {
        public string Date { get; set; }
        public string TimeNo { get; set; }
        public string TimeName { get; set; }
        public double Qty { get; set; }
        public object WorkerAvailable { get; set; }
    }
    public class QueueItem
    {
        //public string QNo { get; set; }
        //public string PrdNo { get; set; }
        //public string JobType { get; set; }
        //public string JobNo { get; set; }
        //public string ShippointManage { get; set; }
        //public string VenderGroupNo { get; set; }
        //public string QStyle { get; set; }
        //public string IsOnOrder { get; set; }
        //public List<ResponseDataItem> DataItems { get; set; }
        //public string Pattype { get; set; }
        //public string Patkey { get; set; }

        public string QNo { get; set; }
        public string PrdNo { get; set; }
        public string JobType { get; set; }
        public string JobNo { get; set; }
        public string ShippointManage { get; set; }
        public string VenderGroupNo { get; set; }
        public string QStyle { get; set; }
        public string IsOnOrder { get; set; }
        public List<QueueDataItem> DataItems { get; set; }
        public string? Pattype { get; set; }
        public string? Patkey { get; set; }
    }
    public class QueueDataItem
    {
        public string Shippoint { get; set; }
        public string LineItem { get; set; }
        public string ArtNo { get; set; }
        public string CBNo { get; set; }
        public string DeliverySite { get; set; }
        public string QArtkey { get; set; }
        public string ItemText { get; set; }
        public string VendorId { get; set; }
        public decimal? StockQty { get; set; }
        public decimal Qty { get; set; }
    }
    public class DeliFee
    {
        public string Article { get; set; }
        public string MCH { get; set; }
        public string ArticleList { get; set; }
        public string LineItem { get; set; }
        public string Desc { get; set; }
        public decimal Qty { get; set; }
        public string UOM { get; set; }
        public decimal Total { get; set; }
    }

  
}
