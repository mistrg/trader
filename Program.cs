using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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

            //  Database.Items.Add(new DBItem(){Exch="Bi", Pair="BTCEUR",askPrice = 25000, amount= 1});
            //  Database.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 26000, amount= 0.5});
            //  Database.Items.Add(new DBItem(){Exch="Cm", Pair="BTCEUR",bidPrice = 23000, amount= 0.5});



            var t = new Task(() =>
            {
                while (true)
                {
                    Thread.Sleep(60000);
                    Console.WriteLine("Profit since start" + Database.Items.Sum(p=>p.profit));


                }
            });
            t.Start();


            while (true)
            {
                Thread.Sleep(1000);



                foreach (var item in Database.Items.Where(p => !p.InPosition))
                {

                    var profit = Database.Items.Where(p => p.Exch != item.Exch && p.Pair == item.Pair && (item.askPrice < p.bidPrice) && !p.InPosition);
                    foreach (var p in profit)
                    {
                        var amount = Math.Min(item.amount, p.amount);

                        var profitAbs = Math.Round(p.bidPrice.Value * amount - item.askPrice.Value * amount, 2);
                        var profitRate = Math.Round(100 * profitAbs / (p.bidPrice.Value * amount), 2);


                        if (0.4 <= profitRate && profitRate < 0.8)
                        {
                            Console.ForegroundColor = ConsoleColor.Yellow;
                            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm")} Buy {item.Pair} on {item.Exch} for {Math.Round(item.askPrice.Value * amount, 2)} and sell on {p.Exch} for {Math.Round(p.bidPrice.Value * amount, 2)} and make {profitAbs} profit ({profitRate}%) - duration: {p.Duration}");
                            Console.ResetColor();
                            p.InPosition = true;
                            item.InPosition = true;
                            p.profit = profitAbs;

                        }
                        else if (0.8 <= profitRate)
                        {
                            Console.ForegroundColor = ConsoleColor.Green;
                            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm")} Buy {item.Pair} on {item.Exch} for {Math.Round(item.askPrice.Value * amount, 2)} and sell on {p.Exch} for {Math.Round(p.bidPrice.Value * amount, 2)} and make {profitAbs} profit ({profitRate}%) - duration: {p.Duration}");
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
