using System;

namespace Trader
{

    public class DBItem
    {

        public DBItem()
        {
            StartDate = DateTime.Now;
        }
        public string Pair { get; set; }
        public DateTime StartDate { get; set; }

        public double TakerFeeRate { get; set; }
        
        public double? askPrice { get; set; }
        public double? bidPrice { get; set; }
        public double amount { get; set; }
        public bool InPosition { get; set; }
        public string Exch { get; set; }
    }
}
