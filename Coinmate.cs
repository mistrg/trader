using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

    private string uri = "wss://coinmate.io/api/websocket/channel/order-book/";

    public List<string> Pairs { get; }


    public Coinmate()
    {
        Pairs = new List<string>() { "BTC_EUR", "ETH_EUR" };
    }

    public void ListenToOrderbook(CancellationToken stoppingToken)
    {


        foreach (var pair in Pairs)
        {

            var t = new Task(async () =>
            {
                while (!stoppingToken.IsCancellationRequested)
                {

                    using (var socket = new ClientWebSocket())
                        try
                        {
                            await socket.ConnectAsync(new Uri(uri + pair), stoppingToken);

                            await Receive(socket, stoppingToken, pair);

                        }
                        catch
                        {
                        }
                }
            }, stoppingToken);

            t.Start();


        }
    }


    private async Task Send(ClientWebSocket socket, string data, CancellationToken stoppingToken) =>
        await socket.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Binary, true, stoppingToken);




    private async Task Receive(ClientWebSocket socket, CancellationToken stoppingToken, string pair)
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
                    foreach (var x in res.payload.asks)
                    {
                        var dbEntry = Database.Items.SingleOrDefault(p => p.Exch == nameof(Coinmate) && p.Pair == pair && p.amount == x.amount && p.askPrice == x.price);
                        if (dbEntry == null)
                            Database.Items.Add(new DBItem() { Exch = nameof(Coinmate), Pair = pair, amount = x.amount, askPrice = x.price, StartDate = DateTime.Now });
                    }


                    foreach (var x in res.payload.bids)
                    {
                        var dbEntry = Database.Items.SingleOrDefault(p => p.Exch == nameof(Coinmate) && p.Pair == pair && p.amount == x.amount && p.bidPrice == x.price);
                        if (dbEntry == null)
                            Database.Items.Add(new DBItem() { Exch = nameof(Coinmate), Pair = pair, amount = x.amount, bidPrice = x.price, StartDate = DateTime.Now });
                    }

                    foreach (var w in Database.Items.Where(p => p.Exch == nameof(Coinmate) && p.Pair == pair))
                    {
                        var askItem = res.payload.asks.SingleOrDefault(p => p.amount == w.amount && p.price == w.askPrice);

                        var bidItem = res.payload.bids.SingleOrDefault(p => p.amount == w.amount && p.price == w.bidPrice);

                        if (askItem == null && bidItem == null)
                            w.EndDate = DateTime.Now;

                    }
                }
            }
        };
    }
}


