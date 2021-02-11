using System.Collections.Generic;

namespace Trader.Coinmate
{
    public class BalanceResponse
    {

        public bool error { get; set; }
        public string errorMessage { get; set; }
        public Dictionary<string, Currency> data { get; set; }
        public class Currency
        {
            public string currency { get; set; }
            public double balance { get; set; }
            public double reserved { get; set; }
            public double available { get; set; }

        }

    }
}