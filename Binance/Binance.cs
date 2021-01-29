using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Trader;
using System.Linq;

public class Binance
{
    public List<string> Pairs { get; }


    public Binance()
    {
        Pairs = new List<string>() { "BTCEUR", "ETHEUR" };
    }

    static string uriBI = "https://api.binance.com/api/v3/depth";
    private static readonly HttpClient httpClient = new HttpClient();

    public void ListenToOrderbook(CancellationToken stoppingToken)
    {

        foreach (var pair in Pairs)
        {

            var t = new Task(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {

                    var res = await httpClient.GetFromJsonAsync<BIResult>(uriBI + "?symbol=" + pair);




                    foreach (var x in res.asks)
                    {
                        var amount = double.Parse(x[1]);
                        var price = double.Parse(x[0]);
                        var dbEntry = Database.Items.SingleOrDefault(p => p.Exch == nameof(Binance) && p.Pair == pair && p.amount == amount && p.askPrice == price);
                        if (dbEntry == null)
                            Database.Items.Add(new DBItem() { Exch = nameof(Binance), Pair = pair, amount = amount, askPrice = price });
                    }


                    foreach (var x in res.bids)
                    {
                        var amount = double.Parse(x[1]);
                        var price = double.Parse(x[0]);

                        var dbEntry = Database.Items.SingleOrDefault(p => p.Exch == nameof(Binance) && p.Pair == pair && p.amount == amount && p.bidPrice == price);
                        if (dbEntry == null)
                            Database.Items.Add(new DBItem() { Exch = nameof(Binance), Pair = pair, amount = amount, bidPrice = price });
                    }

                    foreach (var w in Database.Items.Where(p => p.Exch == nameof(Binance) && p.Pair == pair))
                    {
                        var askItem = res.asks.SingleOrDefault(p => p[1] == w.amount.ToString() && p[0] == w.askPrice.ToString());

                        var bidItem = res.bids.SingleOrDefault(p => p[1]== w.amount.ToString() && p[0]== w.bidPrice.ToString());

                        if (askItem == null && bidItem == null)
                            w.EndDate = DateTime.Now;

                    }

                    Thread.Sleep(1000);
                }
            }, stoppingToken);

            t.Start();

        }

    }

}