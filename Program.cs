﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Trader
{
    class Program
    {



        public static string RunId;
        public static int Version;
        static void Main(string[] args)
        {
            Console.ResetColor();

            RunId = DateTime.Now.ToString("yyyyMMddHHmmss");
            Version = 4;

            Console.WriteLine($"Trader version {Version} starting runId: {RunId}!");


            new Coinmate().ListenToOrderbook(CancellationToken.None);

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


                        if (profitReal < 1)
                            continue;


                        Console.ForegroundColor = ConsoleColor.Green;
                        CreateTrade(item, amount, p, profitAbs, profitReal, profitRate, buyFee, sellFee);
                        Console.ResetColor();


                    }
                }
            }


        }
        private static void CreateTrade(DBItem buy, double amount, DBItem sell, double profitAbs, double profitReal, double profitRate, double buyFee, double sellFee)
        {
            Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Buy {buy.Pair} on {buy.Exch} for {Math.Round(buy.askPrice.Value * amount, 2)} and sell on {sell.Exch} for {Math.Round(sell.bidPrice.Value * amount, 2)} and make {profitReal} profit ({profitRate}%)");
            sell.InPosition = true;
            buy.InPosition = true;
            MongoDatabase.Instance.WriteTrade(new Trade()
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
                BotVersion = Version,
                BotRunId = RunId
            });

        }

    }
}
