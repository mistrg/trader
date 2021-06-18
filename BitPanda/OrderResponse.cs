using System;
using System.Collections.Generic;

namespace  Trader.BitPanda
{
    
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
   
    public class OrderResponse
    {
        public Order order { get; set; }
        public List<Trade> trades { get; set; }



         public class Order
    {
        public string time_in_force { get; set; }
        public bool is_post_only { get; set; }
        public string order_id { get; set; }
        public string account_id { get; set; }
        public string instrument_code { get; set; }
        public DateTime time { get; set; }
        public string side { get; set; }
        public string price { get; set; }
        public string amount { get; set; }
        public string filled_amount { get; set; }
        public string type { get; set; }
        public long sequence { get; set; }
        public string status { get; set; }
        public string average_price { get; set; }
        public int order_book_sequence { get; set; }
        public DateTime time_last_updated { get; set; }
        public int update_modification_sequence { get; set; }

        public double priceNum { get { return string.IsNullOrWhiteSpace(price) ? 0 : double.Parse(price); } }
        public double amountNum { get { return string.IsNullOrWhiteSpace(amount) ? 0 : double.Parse(amount); } }
        public double filled_amountNum { get { return string.IsNullOrWhiteSpace(filled_amount) ? 0 : double.Parse(filled_amount); } }

    }

    public class Fee
    {
        public string fee_amount { get; set; }
        public string fee_currency { get; set; }
        public string fee_percentage { get; set; }
        public string fee_group_id { get; set; }
        public string fee_type { get; set; }
        public string running_trading_volume { get; set; }
        public string collection_type { get; set; }
        public double fee_amountNum { get { return string.IsNullOrWhiteSpace(fee_amount) ? 0 : double.Parse(fee_amount); } }
    }

    public class Trade2
    {
        public string trade_id { get; set; }
        public string order_id { get; set; }
        public string account_id { get; set; }
        public string amount { get; set; }
        public string side { get; set; }
        public string instrument_code { get; set; }
        public string price { get; set; }
        public DateTime time { get; set; }
        public int price_tick_sequence { get; set; }
        public long sequence { get; set; }
    }

    public class Trade
    {
        public Fee fee { get; set; }
        public Trade trade { get; set; }
    }

    }




}