using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Trader
{
    class Program
    {



        static async Task Main(string[] args)
        {


            Console.WriteLine("Trader version 2 starting!");
            new Coinmate().ListenToOrderbook(CancellationToken.None);

            new Binance().ListenToOrderbook(CancellationToken.None);

            //  InMemDatabase.Items.Add(new DBItem(){Exch="Bi", Pair="BTCEUR",askPrice = 25000, amount= 1});
            //  InMemDatabase.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 26000, amount= 0.5});
            //  InMemDatabase.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 23000, amount= 0.5});



            var t = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(60000);
                    Console.WriteLine("Profit since start " + Math.Round(InMemDatabase.Items.Sum(p => p.profit), 2) + " Euro");


                }
            });
            t.Start();


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
                            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Buy {item.Pair} on {item.Exch} for {Math.Round(item.askPrice.Value * amount, 2)} and sell on {p.Exch} for {Math.Round(p.bidPrice.Value * amount, 2)} and make {profitAbs} profit ({profitRate}%)");
                            Console.ResetColor();
                            p.InPosition = true;
                            item.InPosition = true;
                            p.profit = profitAbs;

                        }
                        else if (0.8 <= profitRate)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Buy {item.Pair} on {item.Exch} for {Math.Round(item.askPrice.Value * amount, 2)} and sell on {p.Exch} for {Math.Round(p.bidPrice.Value * amount, 2)} and make {profitAbs} profit ({profitRate}%)");
                            Console.ResetColor();
                            p.InPosition = true;
                            item.InPosition = true;
                            p.profit = profitAbs;
                        }


                    }
                }
            }


        }
    }
}
