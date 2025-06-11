using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTestCase
{
    using System;
    using System.Collections.Generic;

    public class JsonDsRequest
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

   
}
