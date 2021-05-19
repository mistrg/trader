using System;
using System.Linq;
using System.Collections.Generic;

namespace Trader.BitBay
{

    public class QueryRequest
    {
        public List<string> markets { get; set; }
        public List<object> limit { get; set; }
        public List<object> offset { get; set; }
        public string fromTime { get; set; }
        public string toTime { get; set; }
        public List<object> userId { get; set; }
        public string offerId { get; set; }
        public List<object> initializedBy { get; set; }
        public List<object> rateFrom { get; set; }
        public List<object> rateTo { get; set; }
        public List<object> userAction { get; set; }
        public List<object> nextPageCursor { get; set; }
    }

    public class TransactionHistoryResponse
    {
        public string status { get; set; }
        public string totalRows { get; set; }
        public List<Order> items { get; set; }
        public QueryResponse query { get; set; }
        public object nextPageCursor { get; set; }

        public double CummulativeFee
        {
            get {
                if (items!=null)
                {
                    //Should work for buy
                    return items.Sum(p => p.commissionValueNum);
                }
                return 0;
            }
        }
        public List<string> errors { get; set; }

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