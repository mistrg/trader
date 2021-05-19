using System.Collections.Generic;

namespace Trader.BitBay
{

    public class OfferRequest
    {
        public double? amount { get; set; }
        public double? rate { get; set; }
        public double? price { get; set; }
        public string offerType { get; set; }
        public string mode { get; set; }
        public bool postOnly { get; set; }
        public bool fillOrKill { get; set; }
        public bool immediateOrCancel { get; set; }
        public string firstBalanceId { get; set; }
        public string secondBalanceId { get; set; }
    }

}