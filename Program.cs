using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trader.Coinmate;

namespace Trader
{
    class Program
    {



        public static string RunId;
        public static int Version;
        static async Task  Main(string[] args)
        {
            Console.ResetColor();

            RunId = DateTime.Now.ToString("yyyyMMddHHmmss");
            Version = 7;

            Console.WriteLine($"Trader version {Version} starting runId: {RunId}!");
            
            //1. BuyInstant  / BuyLimit´=> OrderId
            //2. GetOrderByOrderIdAsync(OrderId) => status????
                //3a. Sell on Binanance
                //3b. Timeout 5sec  status!= 'filled'  ´=> CancelOrder (OrderId)



            //await new CoinmateLogic().GetOrderByOrderIdAsync("BTC_EUR");
            //await new CoinmateLogic().GetOrderByOrderIdAsync(996950149);
            //return;

            new CoinmateLogic().ListenToOrderbook(CancellationToken.None);

            new Binance().ListenToOrderbook(CancellationToken.None);

            //  InMemDatabase.Items.Add(new DBItem(){Exch="Bi", Pair="BTCEUR",askPrice = 25000, amount= 1});
            //  InMemDatabase.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 26000, amount= 0.5});
            //  InMemDatabase.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 23000, amount= 0.5});


           


            while (true)
            {
                Thread.Sleep(1000);



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

                        //profitAbs = profitAbs * (1 - (0.45 / profitRate));


                        if (profitReal <= 0)
                            continue;


                        Console.ForegroundColor = ConsoleColor.Green;
                        CreateOrderCandidate(item, amount, p, profitAbs, profitReal, profitRate, buyFee, sellFee);
                        Console.ResetColor();


                    }
                }
            }


        }
        private static void CreateOrderCandidate(DBItem buy, double amount, DBItem sell, double profitAbs, double profitReal, double profitRate, double buyFee, double sellFee)
        {
            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Buy {buy.Pair} on {buy.Exch} for {Math.Round(buy.askPrice.Value * amount, 2)} and sell on {sell.Exch} for {Math.Round(sell.bidPrice.Value * amount, 2)} and make {profitReal} profit ({profitRate}%)");
            sell.InPosition = true;
            buy.InPosition = true;
            MongoDatabase.Instance.CreateOrderCandidate(new OrderCandidate()
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
            });

        }

    }
}
