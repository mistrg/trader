using System.Collections.Generic;

namespace Trader.BitBay
{

    public class BalanceRoot
    {
        public string status { get; set; }
        public List<Balance> balances { get; set; }
        public List<string> errors { get; set; }

    }

    public class Balance
    {
        public string id { get; set; }
        public string userId { get; set; }
        public double availableFunds { get; set; }
        public double totalFunds { get; set; }
        public double lockedFunds { get; set; }
        public string currency { get; set; }
        public string type { get; set; }
        public string name { get; set; }
        public string balanceEngine { get; set; }
    }
}