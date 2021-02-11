using System;
using Microsoft.EntityFrameworkCore;
using Trader.Binance;
using Trader.Coinmate;
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
                BotRunId = App.RunId,
                BotVersion = App.Version,
                BuyExchange = orderCandidate.BuyExchange,
                SellExchange = orderCandidate.SellExchange,
                EstBuyFee = orderCandidate.EstBuyFee,
                EstProfitGross = orderCandidate.EstProfitGross,
                EstProfitNet = orderCandidate.EstProfitNet,
                EstProfitNetRate = orderCandidate.EstProfitNetRate,
                EstSellFee = orderCandidate.EstSellFee,
                Ocid = orderCandidate.Id,
                Pair = orderCandidate.Pair
            };
            return arbitrage;

        }

        internal void EnrichBuy(Arbitrage arbitrage, Order result)
        {
            if (result != null && arbitrage != null)
            {
                arbitrage.BuyOrderId = result.id;
                arbitrage.BuyOrginalAmount = result.originalAmount;
                arbitrage.BuyRemainingAmount = result.remainingAmount;
                arbitrage.BuyType = result.orderTradeType;
                arbitrage.BuyStatus = result.status;
                arbitrage.BuyUnitPrice = result.price;

                arbitrage.BuyWhenCreated = Helper.UnixTimeStampToDateTime(result.timestamp);


                arbitrage.BuyNetPriceCm = arbitrage.BuyUnitPrice * (arbitrage.BuyOrginalAmount - arbitrage.BuyRemainingAmount); // mozna - poplatky
            }


        }

        internal void EnrichSell(Arbitrage arbitrage, OrderResponse result)
        {
            if (result != null && arbitrage != null)
            {
                arbitrage.SellOrigQty = result.origQtyNum;
                arbitrage.SellOrderListId = result.orderListId;

                arbitrage.SellPrice = result.priceNum;

                arbitrage.SellStatus = result.status;
                arbitrage.SellTimeInForce = result.timeInForce;
                arbitrage.SellTransactionTime = Helper.UnixTimeStampToDateTime(result.transactTime);
                arbitrage.SellType = result.type;
                arbitrage.SellClientOrderId = result.clientOrderId;
                arbitrage.SellCummulativeQuoteQty = result.cummulativeQuoteQtyNum;
                arbitrage.SellExecutedQty = result.executedQtyNum;
                arbitrage.SellOrderId = result.orderId;




                //SellPriceNetBi SELECT 19.51052363 -  (19.51052363 * 0.001) = 19.49101310637   a."Ocid" = 637486012391027239
                arbitrage.SellNetPriceBi = result.cummulativeQuoteQtyNum - (result.cummulativeQuoteQtyNum * 0.001);


                arbitrage.RealProfitNet =  arbitrage.SellNetPriceBi - arbitrage.BuyNetPriceCm;

            }
        }
    }
}