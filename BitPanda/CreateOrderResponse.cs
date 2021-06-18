using System;

namespace Trader.BitPanda
{
    public class CreateOrderResponse
    {
        public string error { get; set; }
        public string order_id { get; set; }
        public string client_id { get; set; }
        public string account_id { get; set; }
        public string instrument_code { get; set; }
        public DateTime time { get; set; }
        public string side { get; set; }
        public string price { get; set; }
        public string amount { get; set; }
        public string filled_amount { get; set; }
        public string type { get; set; }
        public string time_in_force { get; set; }

        public double filled_amountNum { get { return string.IsNullOrWhiteSpace(filled_amount) ? 0 : double.Parse(filled_amount); } }
        public double amountNum { get { return string.IsNullOrWhiteSpace(amount) ? 0 : double.Parse(amount); } }

    }



}