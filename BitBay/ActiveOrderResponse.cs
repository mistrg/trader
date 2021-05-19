using System.Collections.Generic;

namespace Trader.BitBay
{
    public class Item
    {
        public string market { get; set; }
        public string offerType { get; set; }
        public string id { get; set; }
        public string currentAmount { get; set; }
        public string lockedAmount { get; set; }
        public string rate { get; set; }
        public string startAmount { get; set; }
        public string time { get; set; }
        public bool postOnly { get; set; }
        public string mode { get; set; }
        public string receivedAmount { get; set; }
        public string firstBalanceId { get; set; }
        public string secondBalanceId { get; set; }
    }

    public class ActiveOrderResponse
    {
        public string status { get; set; }
        public List<Item> items { get; set; }

        public List<string> errors { get; set; }

    }


}