using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Trader.Email;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Trader
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            await serviceProvider.GetService<App>().RunAsync();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();

            var config = LoadConfiguration();
            services.AddSingleton(config);

            var x = config.GetSection("SmtpSettings");

            services.Configure<SmtpSettings>(setting => x.Bind(setting));
            services.AddSingleton<IMailer, Mailer>();

            services.AddSingleton<Processor>();
            services.AddSingleton<Presenter>();
            services.AddSingleton<Coinmate.CoinmateLogic>();
            services.AddSingleton<Binance.BinanceLogic>();


            services.AddEntityFrameworkNpgsql().AddDbContext<PostgresContext>(opt =>
           opt.UseNpgsql(Config.PostgresConnectionString));

            // required to run the application
            services.AddTransient<App>();

            return services;
        }

        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

    }
}
