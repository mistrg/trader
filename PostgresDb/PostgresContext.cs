using System;
using Microsoft.EntityFrameworkCore;
using Trader.Infrastructure;

namespace Trader.PostgresDb
{

    public class PostgresContext : DbContext
    {



        public PostgresContext()
        {

        }
        public PostgresContext(DbContextOptions<PostgresContext> options)
  : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Config.PostgresConnectionString);
        }
        public DbSet<Arbitrage> Arbitrages { get; set; }



        internal Arbitrage MakeArbitrageObj(OrderCandidate orderCandidate)
        {
            var arbitrage = new Arbitrage()
            {
                BotRunId = Config.RunId,
                BotVersion = Config.Version,
                BuyExchange = orderCandidate.BuyExchange,
                SellExchange = orderCandidate.SellExchange,
                EstBuyFee = orderCandidate.EstBuyFee,
                EstProfitGross = orderCandidate.EstProfitGross,
                EstProfitNet = orderCandidate.EstProfitNet,
                EstProfitNetRate = orderCandidate.EstProfitNetRate,
                EstSellFee = orderCandidate.EstSellFee,
                Ocid = orderCandidate.Id,
                Pair = orderCandidate.Pair, 
            };
            return arbitrage;

        }

        internal void EnrichBuy(Arbitrage arbitrage, BuyResult result)
        {
            if (result != null && arbitrage != null)
            {
                arbitrage.BuyWhenCreated = Helper.UnixTimeStampToDateTime(result.Timestamp);
                arbitrage.BuyOrderId = result.OrderId;
                arbitrage.BuyComment = result.Comment;
                arbitrage.BuyStatus = result.Status;
                arbitrage.BuyOrginalAmount = result.OriginalAmount;
                arbitrage.BuyRemainingAmount = result.RemainingAmount;
                arbitrage.BuyCummulativeFee = result.CummulativeFee;
                arbitrage.BuyCummulativeFeeQuote = result.CummulativeFeeQuote;
                arbitrage.BuyCummulativeQuoteQty = result.CummulativeQuoteQty;
                arbitrage.BuyUnitPrice = result.Price;

                arbitrage.BuyNetPrice = (arbitrage.BuyCummulativeQuoteQty ?? 0) - (arbitrage.BuyCummulativeFeeQuote ?? 0);
            }


        }

        internal void EnrichSell(Arbitrage arbitrage, SellResult result)
        {
            if (result != null && arbitrage != null)
            {

                arbitrage.SellWhenCreated = Helper.UnixTimeStampToDateTime(result.Timestamp);
                arbitrage.SellOrderId = result.OrderId;
                arbitrage.SellComment = result.Comment;
                arbitrage.SellStatus = result.Status;
                arbitrage.SellOrginalAmount = result.OriginalAmount;
                arbitrage.SellRemainingAmount = result.RemainingAmount;
                arbitrage.SellCummulativeFee = result.CummulativeFee;
                arbitrage.SellCummulativeFeeQuote = result.CummulativeFeeQuote;
                arbitrage.SellCummulativeQuoteQty = result.CummulativeQuoteQty;


                arbitrage.SellNetPrice = (arbitrage.SellCummulativeQuoteQty ?? 0) - (arbitrage.SellCummulativeFeeQuote ?? 0);

                arbitrage.RealProfitNet = (arbitrage.SellCummulativeQuoteQty ?? 0) - (arbitrage.BuyCummulativeQuoteQty ?? 0) - (arbitrage.SellCummulativeFeeQuote ?? 0) - (arbitrage.BuyCummulativeFeeQuote ?? 0);

                arbitrage.RealProfitNetRate = arbitrage.SellNetPrice > 0 ? Math.Round(100 * (arbitrage.RealProfitNet ?? 0) / arbitrage.SellNetPrice.Value, 2) : 0;



            }
        }

     
    }
}