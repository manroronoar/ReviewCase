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
            public double PurchaseAmount { get; set; }
            public string StockAvailType { get; set; }
            public string IntegrateDate { get; set; }
            public List<ReserveDataItems> ReserveDataItems { get; set; }
        }

        public class ReserveDataItems
    {
            public string QStyle { get; set; }
            public string DeliveryDate { get; set; }
            public string InsArticleList { get; set; }
            public List<DataItems> DataItems { get; set; }
        }

        public class DataItems
    {
            public string LineItem { get; set; }
            public string ArtNo { get; set; }
            public string Qty { get; set; }
            public string Unit { get; set; }
            public double UnitPrice { get; set; }
            public double Total { get; set; }
            public double NetPrice { get; set; }
            public string Shippoint { get; set; }
            public string DeliverySite { get; set; }
            public string VendorId { get; set; }
            public TradeInItem TradeInItem { get; set; }
            public string FlagPremium { get; set; }
            public string PremiumPLineItem { get; set; }
            public string FlagMainInstall { get; set; }
            public string InstallPLineItem { get; set; }
        }

        public class TradeInItem
        {
            public string GroupId { get; set; }
            public string GroupDesc { get; set; }
            public double Qty { get; set; }
        }
    }

