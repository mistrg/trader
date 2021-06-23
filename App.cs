using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trader.Binance;
using Trader.Coinmate;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Trader
{
    public class App
    {
        private readonly IConfiguration _config;
        private readonly Processor _processor;
        private readonly PostgresContext _context;
        private readonly Presenter _presenter;
        private readonly Observer _observer;

        private readonly Estimator _estimator;

        private readonly IEnumerable<IExchangeLogic> _exchangeLogics;

        private readonly ObserverContext _ocontext;
        private readonly KeyVaultCache _keyVaultCache;
        public App(IConfiguration config, KeyVaultCache keyVaultCache, Estimator estimator, ObserverContext ocontext, Observer observer, Processor processor, PostgresContext context, IEnumerable<IExchangeLogic> exchangeLogics, Presenter presenter)
        {
            _config = config;
            _processor = processor;
            _ocontext = ocontext;
            _keyVaultCache = keyVaultCache;
            _context = context;
            _exchangeLogics = exchangeLogics;
            _presenter = presenter;
            _observer = observer;
            _estimator = estimator;
        }

        public async Task RunAsync()
        {
            Console.ResetColor();
            _ocontext.NewBotrun();
            _presenter.ShowInfo($"Trader version {Config.Version} starting runId: {Config.RunId}!");


            var bp = _exchangeLogics.Single(p => p.GetType() == typeof(BitPanda.BitPandaLogic));
            var x = await bp.GetAvailableAmountAsync("BTCEUR");

            Console.WriteLine($"BitPanda: {x.Item1} BTC, {x.Item2} Euro");


            var bb = _exchangeLogics.Single(p => p.GetType() == typeof(BitBay.BitBayLogic));
            var y = await bb.GetAvailableAmountAsync("BTCEUR");

            Console.WriteLine($"BitBay: {y.Item1} BTC, {y.Item2} Euro");


            //    //   "BTC-EUR","buy",0.0005,35450
            //     var ocx = new OrderCandidate()
            //     {
            //         Amount = 0.0005, 
            //         UnitAskPrice = 32342,
            //         Pair = "BTCEUR",
            //         BuyExchange = "BitPanda",
            //         SellExchange = "BitBay",

            //     };

            //     await _processor.ProcessOrderAsync(ocx);






            // var dbt = new Task(async () =>
            //                       {
            //                           await _observer.RunAsync();
            //                       });
            // dbt.Start();


            while (true)
            {

                var bob = await bb.GetOrderBookAsync();
                var cob = await bp.GetOrderBookAsync();

                var db = bob.Union(cob);

                var oc = _estimator.Run(db);

                if (oc != null)
                {
                    _presenter.PrintOrderCandidate(oc);


                    var processOrder = Config.AutomatedTrading && oc.EstProfitNetRate > Config.AutomatedTradingMinEstimatedProfitNetRate;
                    if (processOrder)
                        await _processor.ProcessOrderAsync(oc);


                    if (Config.PauseAfterArbitrage)
                    {
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                    }
                }
            }
        }
    }
}
