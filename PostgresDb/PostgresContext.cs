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

                //arbitrage.BuyCommission = result.commissionTotal;


                arbitrage.BuyNetPrice = arbitrage.BuyUnitPrice * (arbitrage.BuyOrginalAmount - arbitrage.BuyRemainingAmount); // mozna - poplatky
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

                arbitrage.SellCommission = result.commissionTotal;



                arbitrage.SellNetPrice = result.cummulativeQuoteQtyNum - (result.commissionTotal ?? 0);


                arbitrage.RealProfitNet = arbitrage.SellNetPrice - arbitrage.BuyNetPrice;

            }
        }
    }
}