using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Trader.Fundamentals;

namespace Trader.PostgresDb
{

    public class Arbitrage
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        //Order candidate
        public double EstProfitGross { get; set; }

        public double EstProfitNet { get; set; }

        public double EstProfitNetRate { get; set; }
        public double EstBuyFee { get; set; }

        public double EstSellFee { get; set; }

        [MaxLengthAttribute(100)]
        public string BotRunId { get; set; }
        public int BotVersion { get; set; }
        public long Ocid { get; set; }


        //Buy exchange

        public long? BuyOrderId { get; set; }

        public DateTime? BuyWhenCreated { get; set; }

        [MaxLengthAttribute(50)]
        public string BuyExchange { get; set; }


        [MaxLengthAttribute(20)]
        public string Pair { get; set; }

        public double? BuyUnitPrice { get; set; }
        public double? BuyOrginalAmount { get; set; }

        public double? BuyRemainingAmount { get; set; }

        [MaxLengthAttribute(50)]
        public string BuyStatus { get; set; }
        [MaxLengthAttribute(50)]
        public string BuyType { get; set; }


        //Buy exchange
        [MaxLengthAttribute(50)]
        public string SellExchange { get; set; }




        [MaxLengthAttribute(1000)]
        public string Comment { get; set; }
        public double? SellOrigQty { get; set; }
        public long? SellOrderListId { get; set; }
        public double? SellPrice { get; set; }

        [MaxLengthAttribute(50)]
        public string SellStatus { get; set; }

        [MaxLengthAttribute(50)]
        public string SellTimeInForce { get; set; }
        public DateTime? SellTransactionTime { get; set; }

        [MaxLengthAttribute(50)]
        public string SellType { get; set; }

        [MaxLengthAttribute(100)]
        public string SellClientOrderId { get; set; }
        public double? SellExecutedQty { get; set; }
        public long? SellOrderId { get; set; }
        public double? SellCummulativeQuoteQty { get; set; }


        public bool IsSuccess { get; set; }
        public double? SellNetPriceBi { get; set; }
        public double? BuyNetPriceCm { get; set; }

        public double? RealProfitNet { get; set; }
        public double BeforeSellExchangeAvailableAmount { get; set; }
        public double BeforeBuyExchangeAvailableAmount { get; set; }
        public double AfterSellExchangeAvailableAmount { get; set; }
        public double AfterBuyExchangeAvailableAmount { get; set; }
    }
}