namespace Trader.Binance
{
    public class OrderResponse
    {
        public string symbol { get; set; }
        public long orderId { get; set; }
        public long orderListId { get; set; }
        public string clientOrderId { get; set; }

        public long transactTime { get; set; }
        public string price { get; set; }
        public string origQty { get; set; }
        public string executedQty { get; set; }
        public string cummulativeQuoteQty { get; set; }
        public string status { get; set; }
        public string timeInForce { get; set; }
        public string type { get; set; }
        public string side { get; set; }


        public double priceNum {get { return string.IsNullOrWhiteSpace(price) ? 0 : double.Parse(price);}}
        public double origQtyNum { get { return string.IsNullOrWhiteSpace(origQty) ? 0 : double.Parse(origQty); } }
        public double executedQtyNum { get { return string.IsNullOrWhiteSpace(executedQty) ? 0 : double.Parse(executedQty); } }
        public double cummulativeQuoteQtyNum { get { return string.IsNullOrWhiteSpace(cummulativeQuoteQty) ? 0 : double.Parse(cummulativeQuoteQty); } }


    }

}