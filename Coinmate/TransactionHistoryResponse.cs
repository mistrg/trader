using System.Collections.Generic;

namespace Trader.Coinmate
{
    public class TransactionHistoryResponse
    {

        public bool error { get; set; }
        public string errorMessage { get; set; }
        public List<Transaction> data { get; set; }

    public class Transaction
    {
        public long timestamp { get; set; }
        public long transactionId { get; set; }
        public string transactionType { get; set; }
        
        public double? price { get; set; }
        public string priceCurrency { get; set; }

        public double? amount { get; set; }
        public string amountCurrency { get; set; }

        public double? fee { get; set; }

        public string feeCurrency { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public long? orderId { get; set; }


    }
}
}

