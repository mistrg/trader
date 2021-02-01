using System.Collections.Generic;

namespace Trader.Coinmate
{
    public class GetOrderHistoryResponse
    {

        public bool error { get; set; }
        public string errorMessage { get; set; }
        public List<OrderHistory> data { get; set; }

    }
}