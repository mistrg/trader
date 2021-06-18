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
        public App(IConfiguration config,KeyVaultCache keyVaultCache, Estimator estimator, ObserverContext ocontext, Observer observer, Processor processor, PostgresContext context, IEnumerable<IExchangeLogic> exchangeLogics, Presenter presenter)
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

            var bb = _exchangeLogics.Single(p => p.GetType() == typeof(BitPanda.BitPandaLogic));
            //var x = await bb.GetAvailableAmountAsync("BTCEUR");
            var f = await bb.GetAvailableAmountAsync("BTCEUR");
            Console.WriteLine($"{f}");


             var ww = _exchangeLogics.Single(p => p.GetType() == typeof(BitBay.BitBayLogic));

//            var xe = await ww.GetAvailableAmountAsync("BTCEUR");
            var fa = await ww.GetTradingTakerFeeRateAsync();

            Console.WriteLine($"{fa}");

           //   "BTC-EUR","buy",0.0005,35450
            var ocx = new OrderCandidate()
            {
                Amount = 0.0005, 
                UnitAskPrice = 32342,
                Pair = "BTCEUR",
                BuyExchange = "BitPanda",
                SellExchange = "BitBay",
                
            };

            await _processor.ProcessOrderAsync(ocx);



            //var x = await (bb as BitBayLogic).NewLimitOrderAsync("BTC-EUR","buy",0.0005,35450);

            // var s = await (bb as BitBayLogic).NewLimitOrderAsync("BTC-EUR","sell",0.00049785,35500);
            //x.completed == true and errors empty 
            //? how to get parial buys
            //? commissionValue  how to get fees
            //? better rate?

            //Buy are paid from  0.00000215 BTC => 
            //SELLs are paid from 0.08 Euro





            // var w = await (bb as BitBayLogic).GetTransactionsHistoryAsync("BTC-EUR", "55a59b41-b738-11eb-8513-0242ac110010");

            //How to get specific offerId


            //var x = await (bb as BitBayLogic).Cancel("BTC-EUR","buy",0.0005,30000);



            _presenter.ShowInfo($"Trader version {Config.Version} starting runId: {Config.RunId}!");

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
                Thread.Sleep(1111);
                // var bob = await bl.GetOrderBookAsync();
                // var cob = await cl.GetOrderBookAsync();

                // var db = bob.Union(cob);

                // var oc = _estimator.Run(db);

                // if (oc != null)
                // {
                //     _presenter.PrintOrderCandidate(oc);


                //     var processOrder = Config.AutomatedTrading && oc.EstProfitNetRate > Config.AutomatedTradingMinEstimatedProfitNetRate;
                //     if (processOrder)
                //         await _processor.ProcessOrderAsync(oc);
                // }
            }
        }
    }
}