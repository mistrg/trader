using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.Diagnostics;
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
        private readonly AaxLogic _aaxLogic;
        private readonly Presenter _presenter;

        private static int _dbRetries = 0;

        public static string RunId;
        public static int Version;



        public App(IConfiguration config, Processor processor, PostgresContext context, CoinmateLogic coinmateLogic, BinanceLogic binanceLogic, Presenter presenter, AaxLogic aaxLogic)
        {
            _aaxLogic = aaxLogic;
            _config = config;
            _processor = processor;
            _context = context;
            _coinmateLogic = coinmateLogic;
            _binanceLogic = binanceLogic;
            _presenter = presenter;
        }

        public async Task RunAsync()
        {

            Console.ResetColor();

            RunId = DateTime.Now.ToString("yyyyMMddHHmmss");
            Version = 15;

            _presenter.ShowInfo($"Trader version {Version} starting runId: {RunId}!");

            await _coinmateLogic.PrintAccountInformationAsync();

            await _binanceLogic.PrintAccountInformationAsync();


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
                var bobe = await _binanceLogic.GetOrderBookAsync("BTCUSDT");
                var cob = await _coinmateLogic.GetOrderBookAsync("BTC_EUR");
                var aob = await _aaxLogic.GetOrderBookAsync("BTCUSDT");

                var db = bob.Union(cob).Union(bobe).Union(aob);


                foreach (var buyEntry in db.Where(p => !p.InPosition))
                {
                    var buyLogic = ResolveExchangeLogic(buyEntry.Exch);

                    var sellEntrys = db.Where(sellEntry => sellEntry.Exch != buyEntry.Exch && sellEntry.Pair == buyEntry.Pair && (buyEntry.askPrice < sellEntry.bidPrice) && !sellEntry.InPosition);
                    foreach (var sellEntry in sellEntrys)
                    {
                        if (buyEntry.InPosition || sellEntry.InPosition)
                            continue;

                        var minimalAmount = Math.Round(Math.Min(buyEntry.amount, sellEntry.amount), 6);

                        if (minimalAmount <= 0.0002)
                            continue; //Too small for coinmate

                        var estProfitGross = Math.Round(sellEntry.bidPrice.Value * minimalAmount - buyEntry.askPrice.Value * minimalAmount, 2);


                        var sellLogic = ResolveExchangeLogic(sellEntry.Exch);

                        var estBuyFee = Math.Round(buyEntry.askPrice.Value * minimalAmount * buyLogic.GetTradingTakerFeeRate(), 2);

                        var estSellFee = Math.Round(sellEntry.bidPrice.Value * minimalAmount * sellLogic.GetTradingTakerFeeRate(), 2);


                        var estProfitNet = Math.Round(estProfitGross - estBuyFee - estSellFee, 2);

                        var profitNetRate = Math.Round(100 * estProfitNet / (sellEntry.bidPrice.Value * minimalAmount), 2);

                        if (profitNetRate <= 0)
                            continue;


                        buyEntry.InPosition = true;
                        sellEntry.InPosition = true;


                        await CreateOrderCandidateAsync(buyEntry, sellEntry, minimalAmount, estProfitGross, estProfitNet, profitNetRate, estBuyFee, estSellFee);

                    }
                }

                timer.Stop();

                TimeSpan timeTaken = timer.Elapsed;
                lastCycle = timer.ElapsedMilliseconds;

            }


        }


        private IExchangeLogic ResolveExchangeLogic(string exchange)
        {
            switch (exchange)
            {
                case nameof(Aax):
                    return _aaxLogic;
                case nameof(Coinmate):
                    return _coinmateLogic;
                case nameof(Binance):
                    return _binanceLogic;
                default:
                    throw new Exception("Invalid exchnage");
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
                    _presenter.ShowError($"Retrying didnt help");

                    throw;
                }
                _presenter.ShowInfo($"Retrying Database write {App._dbRetries}/20");
                MongoDatabase.Reset();
            }
        }

    }
}