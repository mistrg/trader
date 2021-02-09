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
        public DbSet<Trade> Trades { get; set; }
    }
}