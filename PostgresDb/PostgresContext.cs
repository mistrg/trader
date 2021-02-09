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

        internal Arbitrage MakeTradeObj(OrderCandidate orderCandidate)
        {
            var trade = new Arbitrage()
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
            return trade;

        }

        internal void EnrichBuy(Arbitrage trade, Order result)
        {
            trade.BuyOrderId = result.id;
            trade.BuyOrginalAmount = result.originalAmount;
            trade.BuyRemainingAmount = result.remainingAmount;
            trade.BuyType = result.orderTradeType;
            trade.BuyStatus = result.status;
            trade.BuyUnitPrice = result.price;
            trade.BuyWhenCreated = Helper.UnixTimeStampToDateTime(result.timestamp);


        }

        internal void EnrichSell(Arbitrage trade, OrderResponse result)
        {
            trade.SellOrigQty = result.origQtyNum;
            trade.SellOrderListId = result.orderListId;

            trade.SellPrice = result.priceNum;

            trade.SellStatus = result.status;
            trade.SellTimeInForce = result.timeInForce;
            trade.SellTransactionTime = Helper.UnixTimeStampToDateTime(result.transactTime);
            trade.SellType = result.type;
            trade.SellClientOrderId = result.clientOrderId;
            trade.SellCummulativeQuoteQty = result.cummulativeQuoteQtyNum;
            trade.SellExecutedQty = result.executedQtyNum;
            trade.SellOrderId = result.orderId;

        }
    }
}