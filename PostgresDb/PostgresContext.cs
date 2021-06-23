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
            var cs = new KeyVaultCache().GetCachedSecret("ProstgresConnectionString");
            
            optionsBuilder.UseNpgsql(cs);
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
                BuyOrginalAmount = orderCandidate.Amount,
                BuyUnitPrice = orderCandidate.TotalAskPrice
            };
            return arbitrage;

        }
    }
}