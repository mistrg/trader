using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trader.Aax;
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
        private readonly CoinmateLogic _coinmateLogic;
        private readonly BinanceLogic _binanceLogic;
        private readonly Presenter _presenter;
        private readonly Observer _observer;

        private readonly Estimator _estimator;


        public App(IConfiguration config, Estimator estimator, Observer observer, Processor processor, PostgresContext context, CoinmateLogic coinmateLogic, BinanceLogic binanceLogic, Presenter presenter)
        {
            _config = config;
            _processor = processor;
            _context = context;
            _coinmateLogic = coinmateLogic;
            _binanceLogic = binanceLogic;
            _presenter = presenter;
            _observer = observer;
            _estimator = estimator;
        }

        public async Task RunAsync()
        {
            Console.ResetColor();

            Config.RunId = DateTime.Now.ToString("yyyyMMddHHmmss");

            _presenter.ShowInfo($"Trader version {Config.Version} starting runId: {Config.RunId}!");


            await _coinmateLogic.PrintAccountInformationAsync();
            await _binanceLogic.PrintAccountInformationAsync();



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


            Thread.Sleep(6000000);
            while (true)
            {

                var bob = await _binanceLogic.GetOrderBookAsync();
                var cob = await _coinmateLogic.GetOrderBookAsync();

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