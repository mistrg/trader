using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Diagnostics;
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
        private readonly CoinmateLogic _coinmateLogic;
        private readonly BinanceLogic _binanceLogic;
        private readonly Presenter _presenter;

        private static int _dbRetries = 0;

        public static string RunId;
        public static int Version;



        public App(IConfiguration config, Processor processor, PostgresContext context, CoinmateLogic coinmateLogic, BinanceLogic binanceLogic, Presenter presenter)
        {
            _config = config;
            _processor = processor;
            _context = context;
            _coinmateLogic = coinmateLogic;
            _binanceLogic = binanceLogic;
            _presenter = presenter;
        }

        public async Task RunAsync()
        {
            var logDirectory = _config.GetValue<string>("Runtime:LogOutputDirectory");
            var logger = new LoggerConfiguration()
                .WriteTo.File(logDirectory)
                .CreateLogger();

            Console.ResetColor();

            RunId = DateTime.Now.ToString("yyyyMMddHHmmss");
            Version = 13;

            Console.WriteLine($"Trader version {Version} starting runId: {RunId}!");

         
            long lastCycle = 0;
            while (true)
            {
                if (lastCycle < 620) //We can query coinmate every 0.6 sec
                {
                    var x = 620 - (int)lastCycle;
                    Thread.Sleep(x);
                }

                var timer = new Stopwatch();
                timer.Start();

                var bob = await _binanceLogic.GetOrderBookAsync("BTCEUR");
                var cob  = await _coinmateLogic.GetOrderBookAsync("BTC_EUR");

                var db = bob.Union(cob);
              

                foreach (var bookItem1 in db.Where(p => !p.InPosition))
                {
                    var bookItem2s = db.Where(bookItem2 => bookItem2.Exch != bookItem1.Exch && bookItem2.Pair == bookItem1.Pair && (bookItem1.askPrice < bookItem2.bidPrice) && !bookItem2.InPosition);
                    foreach (var bookItem2 in bookItem2s)
                    {
                        if (bookItem1.InPosition || bookItem2.InPosition)
                            continue;

                        var minimalAmount = Math.Round(Math.Min(bookItem1.amount, bookItem2.amount), 6);

                        if (minimalAmount <= 0.0002)
                            continue; //Too small for coinmate

                        var estProfitGross = Math.Round(bookItem2.bidPrice.Value * minimalAmount - bookItem1.askPrice.Value * minimalAmount, 2);

                        var estBuyFee = 0;
                        var estSellFee = Math.Round(bookItem2.bidPrice.Value * minimalAmount * 0.001, 2);


                        var estProfitNet = Math.Round(estProfitGross - estBuyFee - estSellFee, 2);

                        var profitNetRate = Math.Round(100 * estProfitNet / (bookItem2.bidPrice.Value * minimalAmount), 2);

                        if (profitNetRate <= 0)
                            continue;


                        bookItem1.InPosition = true;
                        bookItem2.InPosition = true;


                        await CreateOrderCandidateAsync(bookItem1, bookItem2, minimalAmount, estProfitGross, estProfitNet, profitNetRate, estBuyFee, estSellFee);

                    }
                }

                timer.Stop();

                TimeSpan timeTaken = timer.Elapsed;
                lastCycle = timer.ElapsedMilliseconds;

            }


        }

        


        private async Task CreateOrderCandidateAsync(DBItem buy, DBItem sell, double minimalAmount, double estProfitGross, double estProfitNet, double estProfitNetRate, double estBuyFee, double estSellFee)
        {
            var oc = new OrderCandidate()
            {
                WhenBuySpoted = buy.StartDate,
                WhenSellSpoted = sell.StartDate,
                BuyExchange = buy.Exch,
                SellExchange = sell.Exch,
                Pair = buy.Pair,
                UnitAskPrice = buy.askPrice.Value,
                TotalAskPrice = Math.Round(buy.askPrice.Value * minimalAmount, 2),
                Amount = minimalAmount,
                UnitBidPrice = sell.bidPrice.Value,
                TotalBidPrice = Math.Round(sell.bidPrice.Value * minimalAmount, 2),
                EstProfitGross = estProfitGross,
                EstProfitNet = estProfitNet,
                EstProfitNetRate = estProfitNetRate,
                EstBuyFee = estBuyFee,
                EstSellFee = estSellFee,
                BotVersion = Version,
                BotRunId = RunId
            };

            _presenter.PrintOrderCandidate(oc);

            
            var processOrder = Config.AutomatedTrading && oc.EstProfitNetRate > Config.AutomatedTradingMinEstimatedProfitNetRate;
            if (processOrder)
            {
                await _processor.ProcessOrderAsync(oc);
            }

            try
            {
                MongoDatabase.Instance.CreateOrSkipOrderCandidate(oc, processOrder);
                App._dbRetries = 0;
            }
            catch (System.Exception)
            {
                Random r = new Random();
                int rInt = r.Next(1000, 60 * 1000);
                Thread.Sleep(rInt);

                App._dbRetries++;

                if (App._dbRetries > 20)
                {
                    throw;
                }
                Console.WriteLine($"Retrying Database write {App._dbRetries}/20");
                MongoDatabase.Reset();
            }
        }

    }
}