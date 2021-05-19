using System.Collections.Generic;

namespace Trader.BitFlyer
{

    public class Bid
    {
        public double price { get; set; }
        public double size { get; set; }
    }

    public class Ask
    {
        public double price { get; set; }
        public double size { get; set; }
    }

    public class OrderBookResponse
    {
        public double mid_price { get; set; }
        public List<Bid> bids { get; set; }
        public List<Ask> asks { get; set; }
    }
}