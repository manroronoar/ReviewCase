using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTestCase
{
    using System;
    using System.Collections.Generic;

    public class JsonDSRequest
    {
        public string AppChannel { get; set; } = string.Empty;
        public string StoreNo { get; set; } = string.Empty;
        public string SalesStoreNo { get; set; } = string.Empty;
        public string ReserveFlag { get; set; } = string.Empty;
        public string SuggestFlag { get; set; } = string.Empty;
        public string ProvinceName { get; set; } = string.Empty;
        public string DistrictName { get; set; } = string.Empty;
        public string SubDistrictName { get; set; } = string.Empty;
        public string Village { get; set; } = string.Empty;
        public double? PurchaseAmount { get; set; } = null;
        public string StockAvailType { get; set; } = string.Empty;
        public string IntegrateDate { get; set; } = string.Empty;
        public List<ReserveDataItems> ReserveDataItems { get; set; } 
    }

    public class ReserveDataItems
    {
        public string QStyle { get; set; } = string.Empty;
        public string DeliveryDate { get; set; } = string.Empty;
        public string InsArticleList { get; set; } = string.Empty;
        public string TimeType { get; set; } = string.Empty;
        public string TimeNo { get; set; } = string.Empty;
        public List<DataItems> DataItems { get; set; } 
    }

    public class DataItems
    {
        public string LineItem { get; set; } = string.Empty;
        public string ArtNo { get; set; } = string.Empty;
        public string Qty { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public double? UnitPrice { get; set; } = null;
        public double? Total { get; set; } = null;
        public double? NetPrice { get; set; } = null;
        public string Shippoint { get; set; } = string.Empty;
        public string DeliverySite { get; set; } = string.Empty;
        public string VendorId { get; set; } = string.Empty;
        public TradeInItem TradeInItem { get; set; } 
        public string FlagPremium { get; set; } = string.Empty;
        public string PremiumPLineItem { get; set; } = string.Empty;
        public string FlagMainInstall { get; set; } = string.Empty;
        public string InstallPLineItem { get; set; } = string.Empty;
    }

    public class TradeInItem
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupDesc { get; set; } = string.Empty;
        public double? Qty { get; set; } = null;
    }
}

