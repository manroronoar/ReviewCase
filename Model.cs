using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfTestCase
{
    public class CaseType
    {
        public string CaseTypeReviews { get; set; } = string.Empty;
        public bool StatusCase { get; set; } = false;
    }
    public class Order
    {
        public string RunNo { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string MM { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string OrderId { get; set; } = string.Empty;
        public string TicketNo { get; set; } = string.Empty;
        public string IsSameDay { get; set; } = string.Empty;
        public string Delivery { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string OrderError { get; set; } = string.Empty;
        public string Pos { get; set; } = string.Empty;
        public string RootCause { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
        public string Job { get; set; } = string.Empty;
        public string CaseIrNo { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string CaseReviews { get; set; } = string.Empty;
        public bool StatusCase { get; set; } = false;
        public int StatusCode { get; set; }
    }
    public class TbEvents
    {
        public long Id { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public string Req { get; set; } = string.Empty;
        public string Resp { get; set; } = string.Empty;
        public string HttpStatus { get; set; } = string.Empty;
        public string HttpStatusText { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string MarkForDelete { get; set; } = string.Empty;
        public DateTime CreateDate { get; set; } = DateTime.UtcNow;
        public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
        public double LapsedTime { get; set; }
        public long Seq { get; set; }

    }

    public class PatternTbEvents
    {
        public List<TbEvents> tbEvents ;
        public int SeqPattern { get; set; } = 0;
        public int SeqCount { get; set; } = 0;

        public int boxA { get; set; } = 0;

        public int boxB { get; set; } = 0;

        public int boxC { get; set; } = 0;

        public int boxD { get; set; } = 0;

        public int boxE { get; set; } = 0;
    }
    public class Event
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public int Id { get; set; }
        public DateTime CreateDate { get; set; }
        public string Req { get; set; }
    }

    public class ProcessedEvent
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public int Id { get; set; }
        public DateTime EventTime { get; set; }
        public int EventOrder { get; set; }
        public List<string> FullSequence { get; set; }
        public string SequenceStatus { get; set; }
    }
}
