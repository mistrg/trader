using System.Collections.Generic;

namespace Trader.Binance
{
    public class AccountInfo
    {
        public int makerCommission { get; set; }
        public int takerCommission { get; set; }
        public int buyerCommission { get; set; }
        public int sellerCommission { get; set; }
        public bool canTrade { get; set; }
        public bool canWithdraw { get; set; }
        public bool canDeposit { get; set; }
        public long updateTime { get; set; }
        public string accountType { get; set; }
        public List<Balances> balances { get; set; }
        public List<string> permissions { get; set; }


        public class Balances
        {
            public string asset { get; set; }
            public string free { get; set; }
            public string locked { get; set; }
            public double freeNum { get { return string.IsNullOrWhiteSpace(free) ? 0 : double.Parse(free); } }
            public double lockedNum { get { return string.IsNullOrWhiteSpace(locked) ? 0 : double.Parse(locked); } }
        }
    }

}