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
        public string AppChannel { get; set; }
        public string StoreNo { get; set; }
        public string SalesStoreNo { get; set; }
        public string ReserveFlag { get; set; }
        public string SuggestFlag { get; set; }
        public string ProvinceName { get; set; }
        public string DistrictName { get; set; }
        public string SubDistrictName { get; set; }
        public string Village { get; set; }
        public decimal PurchaseAmount { get; set; }
        public string StockAvailType { get; set; }
        public string IntegrateDate { get; set; }
        public List<string> QStyle { get; set; }
        public List<ReserveDataItems> ReserveDataItems { get; set; }
    }

    public class ReserveDataItems
    {
        //public string QStyle { get; set; } = string.Empty;
        //public string DeliveryDate { get; set; } = string.Empty;
        //public string InsArticleList { get; set; } = string.Empty;
        //public string TimeType { get; set; } = string.Empty;
        //public string TimeNo { get; set; } = string.Empty;
        //public List<DataItems> DataItems { get; set; } 

        public string QStyle { get; set; } = string.Empty;
        public string TimeType { get; set; } = string.Empty;
        public string TimeNo { get; set; } = string.Empty;
        public string InsArticleList { get; set; } = string.Empty;
        public string DeliveryDate { get; set; }
        public string RefNo2 { get; set; }
        public List<DataItems> DataItems { get; set; }
    }

    public class DataItems
    {
        //public string LineItem { get; set; } = string.Empty;
        //public string ArtNo { get; set; } = string.Empty;
        //public string Qty { get; set; } = string.Empty;
        //public string Unit { get; set; } = string.Empty;
        //public double? UnitPrice { get; set; } = null;
        //public double? Total { get; set; } = null;
        //public double? NetPrice { get; set; } = null;
        //public string Shippoint { get; set; } = string.Empty;
        //public string DeliverySite { get; set; } = string.Empty;
        //public string VendorId { get; set; } = string.Empty;
        //public TradeInItem TradeInItem { get; set; } 
        //public string FlagPremium { get; set; } = string.Empty;
        //public string PremiumPLineItem { get; set; } = string.Empty;
        //public string FlagMainInstall { get; set; } = string.Empty;
        //public string InstallPLineItem { get; set; } = string.Empty;

        public string LineItem { get; set; }
        public string ArtNo { get; set; }
        public string Qty { get; set; }
        public string Unit { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
        public decimal NetPrice { get; set; }
        public string Shippoint { get; set; }
        public string DeliverySite { get; set; }
        public string VendorId { get; set; }
        public string FlagPremium { get; set; }
        public string PremiumPLineItem { get; set; }
    }

    public class TradeInItem
    {
        public string GroupId { get; set; } = string.Empty;
        public string GroupDesc { get; set; } = string.Empty;
        public double? Qty { get; set; } = null;
    }
}

