namespace Trader.BitBay
{
    public class FeeResponse
    {
        public string status { get; set; }
        public Config config { get; set; }

        public class Commissions
        {
            public string maker { get; set; }
            public string taker { get; set; }
        }

        public class Buy
        {
            public Commissions commissions { get; set; }
        }

        public class Sell
        {
            public Commissions commissions { get; set; }
        }

        public class First
        {
            public string balanceId { get; set; }
            public string minValue { get; set; }
        }

        public class Second
        {
            public string balanceId { get; set; }
            public string minValue { get; set; }
        }

        public class Config
        {
            public Buy buy { get; set; }
            public Sell sell { get; set; }
            public First first { get; set; }
            public Second second { get; set; }
        }
    }


}