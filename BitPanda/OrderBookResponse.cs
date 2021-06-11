using System;
using System.Collections.Generic;

namespace Trader.BitPanda
{



    public class OrderBookResponse
    {
        public string instrument_code { get; set; }
        public DateTime time { get; set; }
        public List<Bid> bids { get; set; }
        public List<Ask> asks { get; set; }
        public int sequence { get; set; }


        public class Bid
        {
            public string price { get; set; }
            public string amount { get; set; }
            public string order_id { get; set; }
        }

        public class Ask
        {
            public string price { get; set; }
            public string amount { get; set; }
            public string order_id { get; set; }
        }
    }

}