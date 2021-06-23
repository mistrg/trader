using System;
using System.Linq;
using System.Collections.Generic;

namespace Trader.BitBay
{
    public class TransactionHistoryResponse
    {
        public string status { get; set; }
        public string totalRows { get; set; }
        public List<Order> items { get; set; }
        public QueryResponse query { get; set; }
        public object nextPageCursor { get; set; }

     
        public List<string> errors { get; set; }

        public class Order
        {
            public string id { get; set; }
            public string market { get; set; }
            public string time { get; set; }
            public string amount { get; set; }
            public string rate { get; set; }
            public string initializedBy { get; set; }
            public bool wasTaker { get; set; }
            public string userAction { get; set; }
            public string offerId { get; set; }
            public string commissionValue { get; set; }

            public double commissionValueNum { get { return string.IsNullOrWhiteSpace(commissionValue) ? 0 : double.Parse(commissionValue); } }
            public double amountNum { get { return string.IsNullOrWhiteSpace(amount) ? 0 : double.Parse(amount); } }
            public double rateNum { get { return string.IsNullOrWhiteSpace(rate) ? 0 : double.Parse(rate); } }
            public double timeNum { get { return string.IsNullOrWhiteSpace(time) ? 0 : double.Parse(time); } }

        }

        public class QueryResponse
        {
            public List<List<string>> markets { get; set; }
            public List<object> limit { get; set; }
            public List<object> offset { get; set; }
            public List<string> fromTime { get; set; }
            public List<object> toTime { get; set; }
            public List<object> userId { get; set; }
            public List<string> offerId { get; set; }
            public List<object> initializedBy { get; set; }
            public List<object> rateFrom { get; set; }
            public List<object> rateTo { get; set; }
            public List<object> userAction { get; set; }
            public List<object> nextPageCursor { get; set; }
        }

    }

}