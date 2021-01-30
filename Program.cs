using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Trader
{
    class Program
    {

        public static MongoDatabase mongoDB = new MongoDatabase();

        static async Task Main(string[] args)
        {


            Console.WriteLine("Trader version 2 starting!");


            new Coinmate().ListenToOrderbook(CancellationToken.None);

            new Binance().ListenToOrderbook(CancellationToken.None);

            //  InMemDatabase.Items.Add(new DBItem(){Exch="Bi", Pair="BTCEUR",askPrice = 25000, amount= 1});
            //  InMemDatabase.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 26000, amount= 0.5});
            //  InMemDatabase.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 23000, amount= 0.5});


         

            while (true)
            {
                Thread.Sleep(1000);



                foreach (var item in InMemDatabase.Items.Where(p => !p.InPosition))
                {
                    var profit = InMemDatabase.Items.Where(p => p.Exch != item.Exch && p.Pair == item.Pair && (item.askPrice < p.bidPrice) && !p.InPosition);
                    foreach (var p in profit)
                    {
                        if (item.InPosition)
                            continue;

                        if (p.InPosition)
                            continue;

                        var amount = Math.Min(item.amount, p.amount);

                        var profitAbs = Math.Round(p.bidPrice.Value * amount - item.askPrice.Value * amount, 2);
                        var profitRate = Math.Round(100 * profitAbs / (p.bidPrice.Value * amount), 2);

                        if (profitAbs < 1)
                            continue;

                        if (0.4 <= profitRate && profitRate < 0.8)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            CreateTrade(item, amount, p, profitAbs, profitRate);
                            Console.ResetColor();


                        }
                        else if (0.8 <= profitRate)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            CreateTrade(item, amount, p, profitAbs, profitRate);
                            Console.ResetColor();
                        }


                    }
                }
            }


        }
        private static void CreateTrade(DBItem buy, double amount, DBItem sell, double profitAbs, double profitRate)
        {
            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Buy {buy.Pair} on {buy.Exch} for {Math.Round(buy.askPrice.Value * amount, 2)} and sell on {sell.Exch} for {Math.Round(sell.bidPrice.Value * amount, 2)} and make {profitAbs} profit ({profitRate}%)");
            sell.InPosition = true;
            buy.InPosition = true;
            mongoDB.WriteTrade(new Trade() { BuyId = buy.Id, SellId = sell.Id,WhenBuySpoted = buy.StartDate, WhenSellSpoted = sell.StartDate,BuyExchange = buy.Exch, SellExchange = sell.Exch, Pair = buy.Pair,UnitAskPrice=buy.askPrice.Value, TotalAskPrice = Math.Round(buy.askPrice.Value * amount, 2), Amount= amount, UnitBidPrice =sell.bidPrice.Value ,  TotalBidPrice = Math.Round(sell.bidPrice.Value * amount, 2),ProfitAbs = profitAbs, ProfitRate = profitRate });

        }

    }
}
