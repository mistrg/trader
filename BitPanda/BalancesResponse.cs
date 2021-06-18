using System;
using System.Collections.Generic;

namespace Trader.BitPanda
{

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 


    public class BalancesResponse
    {
        public string account_id { get; set; }
        public List<Balance> balances { get; set; }


        public class Balance
        {
            public string account_id { get; set; }
            public string currency_code { get; set; }
            public string change { get; set; }
            public string available { get; set; }
            public string locked { get; set; }
            public long sequence { get; set; }
            public DateTime time { get; set; }
        }
    }





}