using System.Collections.Generic;
using System.Linq;

namespace Trader.BitBay
{

    public class OfferResponse
    {
        public string status { get; set; }


        //If the order was completed entirely
        public bool completed { get; set; }

        //UUID of the order
        public string offerId { get; set; }
        public List<OfferTransaction> transactions { get; set; }
        public List<string> errors { get; set; }

        public double CompletedAmount
        {
            get
            {
                if (transactions != null)
                {
                    return transactions.Sum(p => p.amountNum);
                }
                return 0;
            }
        }

        public class OfferTransaction
        {
            public string amount { get; set; }

            public double amountNum { get { return string.IsNullOrWhiteSpace(amount) ? 0 : double.Parse(amount); } }

            public string rate { get; set; }
        }
    }

}