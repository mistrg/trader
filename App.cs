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
            var f = await bp.GetTradingTakerFeeRateAsync();

            _presenter.ShowInfo($"BitPanda: {x.Item1} BTC, {x.Item2} Euro, Fee: {f}");


            var bb = _exchangeLogics.Single(p => p.GetType() == typeof(BitBay.BitBayLogic));
            var y = await bb.GetAvailableAmountAsync("BTCEUR");
            var fe = await bb.GetTradingTakerFeeRateAsync();

            _presenter.ShowInfo($"BitBay: {y.Item1} BTC, {y.Item2} Euro, Fee: {fe}");

            var dbt = new Task(async () =>
                                  {
                                      await _observer.RunAsync();
                                  });
            dbt.Start();


            while (true)
            {
                Thread.Sleep(10 * 1000);
            }

            //     var bob = await bb.GetOrderBookAsync();
            //     var cob = await bp.GetOrderBookAsync();

            //     var db = bob.Union(cob);

            //     var oc = _estimator.Run(db);

            //     if (oc != null)
            //     {

            //         var isDuplicate = await _ocontext.CreateOrSkipOrderCandidateAsync(oc);
            //         if (!isDuplicate)
            //         {

            //             _presenter.PrintOrderCandidate(oc);



            //             var processOrder = Config.AutomatedTrading && oc.EstProfitNetRate > Config.AutomatedTradingMinEstimatedProfitNetRate;
            //             if (processOrder)
            //             {
            //                 await _processor.ProcessOrderAsync(oc);


            //                 if (Config.PauseAfterArbitrage)
            //                 {
            //                     _presenter.ShowInfo("Press any key to continue...");
            //                     //Console.ReadKey();
            //                 }
            //             }
            //         }
            //     }
            // }
        }
    }
}
