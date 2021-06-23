using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Trader.Infrastructure;

namespace Trader.PostgresDb
{
    public class ObserverContext : DbContext
    {

        private List<OrderCandidate> last4000OrderCandidates = new List<OrderCandidate>();


        public DbSet<OrderCandidate> OrderCandidates { get; set; }

        public DbSet<BotRun> BotRuns { get; set; }

        public DbSet<ExchangeRuntimeInfo> ExchangeRuntimeInfo {get;set;}


        

        public ObserverContext()
        {
        }

        public ObserverContext(DbContextOptions<ObserverContext> options)
  : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var cs = new KeyVaultCache().GetCachedSecret("ProstgresConnectionString");
            
            optionsBuilder.UseNpgsql(cs);
        }



        public async Task<bool> CreateOrSkipOrderCandidateAsync(OrderCandidate obj)
        {

            if (last4000OrderCandidates.Any(p => p.BuyExchange == obj.BuyExchange && p.SellExchange == obj.SellExchange && p.Pair == obj.Pair && p.Amount == obj.Amount))
            {
                //Duplicate offer 
                return true;
            }

            last4000OrderCandidates.Add(obj);

            var oversize = last4000OrderCandidates.Count - 4000;
            if (oversize > 0)
                last4000OrderCandidates.RemoveRange(0, oversize);


            await OrderCandidates.AddAsync(obj);
            await SaveChangesAsync();
            return false;


        }

        internal void NewBotrun()
        {
            Config.RunId = DateTime.Now.ToString("yyyyMMddHHmmss");
            var br = new BotRun()
            {
                Id = Config.RunId,
                Version = Config.Version
            };
            BotRuns.Add(br);
            SaveChanges();
        }
    }

}
