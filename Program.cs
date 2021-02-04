using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trader.Binance;
using Trader.Coinmate;

namespace Trader
{
    class Program
    {

        public static int dbRetries = 0;



        public static string RunId;
        public static int Version;
        static async Task Main(string[] args)
        {

            Console.ResetColor();

            RunId = DateTime.Now.ToString("yyyyMMddHHmmss");
            Version = 8;

            Console.WriteLine($"Trader version {Version} starting runId: {RunId}!");


            await TestSuite.TestLowBuyAsync();

            return;

            new CoinmateLogic().ListenToOrderbook(CancellationToken.None);

            new BinanceLogic().ListenToOrderbook(CancellationToken.None);

            while (true)
            {
                Thread.Sleep(300);

                if (Console.KeyAvailable)
                {
                    if (Console.ReadKey(true).Key == ConsoleKey.B)
                    {
                        await CandidateSelectionAsync();
                    }
                }



                foreach (var item in InMemDatabase.Instance.Items.Where(p => !p.InPosition))
                {
                    var profit = InMemDatabase.Instance.Items.Where(p => p.Exch != item.Exch && p.Pair == item.Pair && (item.askPrice < p.bidPrice) && !p.InPosition);
                    foreach (var p in profit)
                    {
                        if (item.InPosition || p.InPosition)
                            continue;

                        var amount = Math.Min(item.amount, p.amount);

                        var profitAbs = Math.Round(p.bidPrice.Value * amount - item.askPrice.Value * amount, 2);

                        var buyFee = p.bidPrice.Value * amount * 0.0035;
                        var sellFee = item.askPrice.Value * amount * 0.001;


                        var profitReal = Math.Round(profitAbs - buyFee - sellFee, 2);

                        var profitRate = Math.Round(100 * profitReal / (p.bidPrice.Value * amount), 2);

                        if (profitReal <= 0)
                            continue;

                        CreateOrderCandidate(item, amount, p, profitAbs, profitReal, profitRate, buyFee, sellFee);


                    }
                }
            }


        }


        public static async Task CandidateSelectionAsync()
        {
            Console.WriteLine($"Please enter OCID:");
            var offerCandidateId = long.Parse(Console.ReadLine());
            OrderCandidate orderCandidate = null;
            if (InMemDatabase.Instance.OrderCandidates.TryGetValue(offerCandidateId, out orderCandidate) && orderCandidate != null)
            {
                Console.WriteLine($"Do you wish to process following order? ");
                Console.ForegroundColor = ConsoleColor.Blue;
                PrintOrderCandidate(orderCandidate);
                Console.ResetColor();

                Console.WriteLine($"(y = yes / n = no) ");


                var process = Console.ReadLine();
                if (process == "y")
                    await Processor.ProcessOrderAsync(orderCandidate);
                else
                    Console.WriteLine("Process cancelled. Continue...");

            }
            else
            {
                Console.WriteLine($"OrderCandidate: {offerCandidateId} not found. Continue...");

            }
        }

        private static void PrintOrderCandidate(OrderCandidate oc)
        {
            Console.WriteLine($"{oc.WhenCreated.ToString("dd.MM.yyyy HH:mm:ss")} OCID: {oc.Id} Buy {oc.Pair} on {oc.BuyExchange} for {oc.TotalAskPrice} and sell on {oc.SellExchange} for {oc.TotalBidPrice} and make {oc.ProfitReal} profit ({oc.ProfitRate}%)");

        }
        private static void CreateOrderCandidate(DBItem buy, double amount, DBItem sell, double profitAbs, double profitReal, double profitRate, double buyFee, double sellFee)
        {
            sell.InPosition = true;
            buy.InPosition = true;

            var oc = new OrderCandidate()
            {
                BuyId = buy.Id,
                SellId = sell.Id,
                WhenBuySpoted = buy.StartDate,
                WhenSellSpoted = sell.StartDate,
                BuyExchange = buy.Exch,
                SellExchange = sell.Exch,
                Pair = buy.Pair,
                UnitAskPrice = buy.askPrice.Value,
                TotalAskPrice = Math.Round(buy.askPrice.Value * amount, 2),
                Amount = amount,
                UnitBidPrice = sell.bidPrice.Value,
                TotalBidPrice = Math.Round(sell.bidPrice.Value * amount, 2),
                ProfitAbs = profitAbs,
                ProfitReal = profitReal,
                ProfitRate = profitRate,
                BuyFee = buyFee,
                SellFee = sellFee,
                BotVersion = Version,
                BotRunId = RunId
            };
            InMemDatabase.Instance.OrderCandidates.Add(oc.Id, oc);


            PrintOrderCandidate(oc);

            try
            {
                MongoDatabase.Instance.CreateOrderCandidate(oc);
                Program.dbRetries = 0;
            }
            catch (System.Exception)
            {
                Random r = new Random();
                int rInt = r.Next(1000, 60 * 1000);
                Thread.Sleep(rInt);

                Program.dbRetries++;

                if (Program.dbRetries > 20)
                {
                    throw;
                }
                Console.WriteLine($"Retrying Database write {Program.dbRetries}/20");
                MongoDatabase.Reset();
            }


        }

    }
}
