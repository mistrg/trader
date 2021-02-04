using System.Collections.Generic;

namespace Trader.Coinmate
{
    public class GetOrderByIdResponse
    {

        public bool error { get; set; }
        public string errorMessage { get; set; }
        public List<Order> data { get; set; }

    }
}