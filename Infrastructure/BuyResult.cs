namespace Trader.Infrastructure
{

    public class BuyResult
    {
        public long Timestamp { get; set; }
        public long OrderId { get; set; }
        public string Comment { get; set; }
        public string Status { get; set; }


        public double? OriginalAmount { get; set; }
        public double? RemainingAmount { get; set; }
        public double CummulativeFee { get; set; }
        public double CummulativeQuoteQty { get; set; }
        public double Price { get; set; }
        public double CummulativeFeeQuote { get; set; }
    }
}
