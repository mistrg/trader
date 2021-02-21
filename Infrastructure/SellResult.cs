namespace Trader.Infrastructure
{

    public class SellResult
    {
        public string Comment { get; set; }

        public long OrderId { get; set; }

        public string Status { get; set; }
        public double? RemainingAmount { get; set; }

        public double? OriginalAmount { get; set; }

        public double Fee { get; set; }
        public string FeeCurrency { get; set; }
    }
}
