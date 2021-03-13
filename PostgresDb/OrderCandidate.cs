using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Trader.PostgresDb
{
    public class OrderCandidate
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        public DateTime WhenCreated { get; set; }

        public DateTime WhenBuySpoted { get; set; }
        public DateTime WhenSellSpoted { get; set; }

        public string BuyExchange { get; set; }
        public string SellExchange { get; set; }
        public string Pair { get; set; }


        public double UnitAskPrice { get; set; }

        public double TotalAskPrice { get; set; }

        public double Amount { get; set; }


        public double UnitBidPrice { get; set; }

        public double TotalBidPrice { get; set; }


        public double EstProfitGross { get; set; }

        public double EstProfitNet { get;  set; }

        public double EstProfitNetRate { get; set; }
        public string BotRunId { get;  set; }
        public int BotVersion { get;  set; }
        public double EstBuyFee { get; set; }

        public double EstSellFee { get; set; }

        public OrderCandidate()
        {
            WhenCreated = DateTime.Now;
        }

    }
}