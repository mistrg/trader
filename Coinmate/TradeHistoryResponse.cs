using System.Collections.Generic;

namespace Trader.Coinmate
{
    public class TradeHistoryResponse
    {
        public long transactionId { get; set; }
        public long createdTimestamp { get; set; }
        public string currencyPair { get; set; }
        public string type { get; set; }
        public string orderType { get; set; }
        public long orderId { get; set; }
        public double amount { get; set; }
        public double price { get; set; }
        public double fee { get; set; }
        public string feeType { get; set; }

    }
}