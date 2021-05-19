namespace Trader.BitBay
{

    public class Order
    {
        public string id { get; set; }
        public string market { get; set; }
        public string time { get; set; }
        public string amount { get; set; }
        public string rate { get; set; }
        public string initializedBy { get; set; }
        public bool wasTaker { get; set; }
        public string userAction { get; set; }
        public string offerId { get; set; }
        public string commissionValue { get; set; }

        public double commissionValueNum { get { return string.IsNullOrWhiteSpace(commissionValue) ? 0 : double.Parse(commissionValue); } }

    }
}