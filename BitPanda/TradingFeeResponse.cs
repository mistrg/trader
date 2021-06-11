using System.Collections.Generic;

namespace Trader.BitPanda
{

    public class TradingFeeResponse
    {
        public string account_id { get; set; }
        public string running_trading_volume { get; set; }
        public string fee_group_id { get; set; }
        public bool collect_fees_in_best { get; set; }
        public string fee_discount_rate { get; set; }
        public string minimum_price_value { get; set; }
        public List<FeeTier> fee_tiers { get; set; }
        public ActiveFeeTier active_fee_tier { get; set; }


        public class FeeTier
        {
            public string volume { get; set; }
            public string fee_group_id { get; set; }
            public string maker_fee { get; set; }
            public string taker_fee { get; set; }
        }

        public class ActiveFeeTier
        {
            public string volume { get; set; }
            public string fee_group_id { get; set; }
            public string maker_fee { get; set; }
            public string taker_fee { get; set; }
        }
    }


}