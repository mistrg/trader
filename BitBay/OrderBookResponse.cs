using System;
using System.Collections.Generic;

namespace Trader.BitBay
{

     public class Sell
        {
            public string ra { get; set; }
            public string ca { get; set; }
            public string sa { get; set; }
            public string pa { get; set; }
            public int co { get; set; }
        }

        public class Buy
        {
            public string ra { get; set; }
            public string ca { get; set; }
            public string sa { get; set; }
            public string pa { get; set; }
            public int co { get; set; }
        }

        public class OrderBookResponse
        {
            public string status { get; set; }
            public List<Sell> sell { get; set; }
            public List<Buy> buy { get; set; }
            public string timestamp { get; set; }
            public string seqNo { get; set; }
        }
}