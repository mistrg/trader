using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class DBItem
{

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    public TimeSpan Duration
    {
        get
        {
            if (EndDate != null)
                return EndDate.Value - StartDate;
            return DateTime.Now - StartDate;
        }
    }

    public double price { get; set; }
    public double amount { get; set; }
    public bool InPosition { get; internal set; }
}
public class Result
{
    public string channel { get; set; }


    [System.Text.Json.Serialization.JsonPropertyName("event")]
    public string Event { get; set; }

    public Payload payload { get; set; }



    public class Payload
    {

        public List<PayAmount> bids { get; set; }
        public List<PayAmount> asks { get; set; }



        public class PayAmount
        {
            public double price { get; set; }
            public double amount { get; set; }
        }

    }


}

public class Coinmate
{

    private string uri = "wss://coinmate.io/api/websocket/channel/order-book/BTC_EUR";

    public List<DBItem> db = new List<DBItem>();



    public double biPrice = 0;

    public async Task ListenToOrderbookAsync(CancellationToken stoppingToken)
    {

        ScheduleTask(async () => biPrice = await Binance.GetPriceAsync(), 1, stoppingToken);


        while (!stoppingToken.IsCancellationRequested)
        {
            using (var socket = new ClientWebSocket())
                try
                {
                    await socket.ConnectAsync(new Uri(uri), CancellationToken.None);
                    Console.WriteLine("socket connected");

                    //await Send(socket, sub, stoppingToken);
                    await Receive(socket, stoppingToken);

                }
                catch (Exception ex)
                {
                    Console.WriteLine($"ERROR - {ex.Message}");
                }
        }

    }


    private async Task Send(ClientWebSocket socket, string data, CancellationToken stoppingToken) =>
        await socket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Binary, true, stoppingToken);


    static void ScheduleTask(Action action, int seconds, CancellationToken token)
    {
        if (action == null)
            return;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                action();
                await Task.Delay(TimeSpan.FromSeconds(seconds), token);
            }
        }, token);
    }


    private async Task Receive(ClientWebSocket socket, CancellationToken stoppingToken)
    {
        var buffer = new ArraySegment<byte>(new byte[2048]);
        while (!stoppingToken.IsCancellationRequested)
        {
            WebSocketReceiveResult result;
            using (var ms = new MemoryStream())
            {
                do
                {
                    result = await socket.ReceiveAsync(buffer, stoppingToken);
                    ms.Write(buffer.Array, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                    break;

                ms.Seek(0, SeekOrigin.Begin);


                var res = await JsonSerializer.DeserializeAsync<Result>(ms);


                if (res.Event == "data" && res.payload != null)
                {

                    foreach (var x in res.payload.bids)
                    {
                        var dbEntry = db.SingleOrDefault(p => p.amount == x.amount && p.price == x.price);
                        if (dbEntry == null)
                            db.Add(new DBItem() { amount = x.amount, price = x.price, StartDate = DateTime.Now });

                    }

                    foreach (var w in db)
                    {
                        var item = res.payload.bids.SingleOrDefault(p => p.amount == w.amount && p.price == w.price);

                        if (item == null)
                            w.EndDate = DateTime.Now;
                    }

                }


                var opened = db.Where(p => p.EndDate == null && !p.InPosition);
                var profitable = opened.Where(x => x.price < biPrice);

                Console.Clear();

                double cashRequired = 0;
                double totalamount = 0;

                foreach (var x in profitable.OrderBy(x => x.Duration))
                {
                    var profitRate = Math.Round((biPrice - x.price) / biPrice * 100, 2);

                    if (profitRate < 0.9)
                    {
                        Console.Write($"Volume: {Math.Round(x.amount * x.price, 2)} EUR  Duration: {x.Duration:hh\\:mm\\:ss}");
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($" Profit: {profitRate}% ");
                        Console.ResetColor();

                    }
                    else if (0.9 <= profitRate && profitRate < 1.4)
                    {
                        Console.Write($"Volume: {Math.Round(x.amount * x.price, 2)} EUR  Duration: {x.Duration:hh\\:mm\\:ss}");
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($" Profit: {profitRate}% ");
                        Console.ResetColor();
                    }
                    else if (1.4 <= profitRate)
                    {
                        Console.Write($"Volume: {Math.Round(x.amount * x.price, 2)} EUR  Duration: {x.Duration:hh\\:mm\\:ss}");
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine($" Profit: {profitRate}% ");
                        Console.ResetColor();
                        cashRequired += Math.Round(x.amount * x.price, 2);
                        totalamount += x.amount;
                        x.InPosition = true;
                    }
                }
                Console.ForegroundColor = ConsoleColor.Green;



                Console.WriteLine($"Curently required cash {Math.Round(cashRequired,0)} Euro, Sellable {Math.Round(totalamount*biPrice)} Euro, Total profit  {Math.Round(totalamount*biPrice) - Math.Round(cashRequired,0)} Euro");
                Console.ResetColor();

            }
        };
    }
}


