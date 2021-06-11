using System.Collections.Generic;

namespace Trader.BitPanda
{

  
    public class WalletResponse
    {
        public List<Datum> data { get; set; }

        public class Attributes
    {
        public string cryptocoin_id { get; set; }
        public string cryptocoin_symbol { get; set; }
        public string balance { get; set; }
        public bool is_default { get; set; }
        public string name { get; set; }
        public int pending_transactions_count { get; set; }
        public bool deleted { get; set; }
    }

    public class Datum
    {
        public string type { get; set; }
        public Attributes attributes { get; set; }
        public string id { get; set; }
    }
    }


    
}