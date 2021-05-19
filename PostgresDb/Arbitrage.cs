using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

        [MaxLengthAttribute(20)]
        public string Pair { get; set; }


        //Buy exchange

        public string BuyOrderId { get; set; }

        public DateTime? BuyWhenCreated { get; set; }

        [MaxLengthAttribute(50)]
        public string BuyExchange { get; set; }

        public double? BuyUnitPrice { get; set; }
        public double? BuyOrginalAmount { get; set; }

        public double? BuyRemainingAmount { get; set; }

        [MaxLengthAttribute(50)]
        public string BuyStatus { get; set; }

        [MaxLengthAttribute(1000)]
        public string BuyComment { get; set; }

        public double? BuyCummulativeQuoteQty { get; set; }

        public double? BuyCummulativeFee { get; set; }
        public double? BuyCummulativeFeeQuote { get; set; }

        public double? BuyNetPrice { get; set; }



        //Sell exchange
        [MaxLengthAttribute(50)]
        public string SellExchange { get; set; }

        public DateTime? SellWhenCreated { get; set; }

        [MaxLengthAttribute(1000)]
        public string SellComment { get; set; }

        public double? SellOrginalAmount { get; set; }
        public double? SellRemainingAmount { get; set; }

        public double? SellCummulativeFee { get; set; }
        public double? SellCummulativeFeeQuote { get; set; }

        public double? SellCummulativeQuoteQty { get; set; }

        [MaxLengthAttribute(50)]
        public string SellStatus { get; set; }

        public long? SellOrderId { get; set; }







        public bool IsSuccess { get; set; }
        public double? SellNetPrice { get; set; }

        public double? RealProfitNet { get; set; }
        public double? RealProfitNetRate { get; set; }

        public double? BeforeBuyExchangeAvailableBaseAmount { get; set; }
        public double? BeforeBuyExchangeAvailableQuoteAmount { get; set; }

        public double? AfterBuyExchangeAvailableBaseAmount { get; set; }
        public double? AfterBuyExchangeAvailableQuoteAmount { get; set; }

        public double? BeforeSellExchangeAvailableBaseAmount { get; set; }
        public double? BeforeSellExchangeAvailableQuoteAmount { get; set; }

        public double? AfterSellExchangeAvailableBaseAmount { get; set; }
        public double? AfterSellExchangeAvailableQuoteAmount { get; set; }

        public double? WalletBaseAmountSurplus { get; set; }
        public double? WalletQuoteAmountSurplus { get;  set; }
    }
}