using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trader.Binance;
using Trader.Coinmate;
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
        public App(IConfiguration config, Estimator estimator,ObserverContext ocontext, Observer observer, Processor processor, PostgresContext context, IEnumerable<IExchangeLogic> exchangeLogics, Presenter presenter)
        {
            _config = config;
            _processor = processor;
            _ocontext = ocontext;
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
            
            var bl = _exchangeLogics.Single(p => p.GetType() == typeof(BinanceLogic));
            var cl = _exchangeLogics.Single(p => p.GetType() == typeof(CoinmateLogic));

            await cl.PrintAccountInformationAsync();
            await bl.PrintAccountInformationAsync();



            //   var buyAvailable = await _coinmateLogic.GetAvailableAmountAsync("BTCEUR"); //Always buying BTC for EUR
            //     var sellAvailable = await _binanceLogic.GetAvailableAmountAsync("BTCEUR"); //Always buying BTC for EUR

            //     Console.WriteLine($"CM {buyAvailable.Item1} BTC         {buyAvailable.Item2} EURO");
            //     Console.WriteLine($"BI {sellAvailable.Item1} BTC         {sellAvailable.Item2} EURO");
            //     return;
            // await _coinmateLogic.GetOrderHistoryAsync("BTC_EUR");

            //await _binanceLogic.GetAllOrders("BTCEUR");
            //    await _binanceLogic.BuyLimitOrderAsync(new OrderCandidate(){Amount = 0.00025, UnitAskPrice = 40500, Pair = "BTCEUR"});
            //await _coinmateLogic.SellMarketAsync(new OrderCandidate(){Amount = 0.00025, Pair = "BTCEUR"});


            var dbt = new Task(async () =>
                                  {
                                      await _observer.RunAsync();
                                  });
            dbt.Start();


            while (true)
            {

                var bob = await bl.GetOrderBookAsync();
                var cob = await cl.GetOrderBookAsync();

                var db = bob.Union(cob);

                var oc = _estimator.Run(db);

                if (oc != null)
                {
                    _presenter.PrintOrderCandidate(oc);


                    var processOrder = Config.AutomatedTrading && oc.EstProfitNetRate > Config.AutomatedTradingMinEstimatedProfitNetRate;
                    if (processOrder)
                        await _processor.ProcessOrderAsync(oc);
                }
            }
        }
    }
}