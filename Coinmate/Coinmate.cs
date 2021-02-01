using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Trader.Coinmate
{

    public class CoinmateLogic
    {

        private static readonly HttpClient httpClient = new HttpClient();

        private string uri = "wss://coinmate.io/api/websocket/channel/order-book/";
        private string baseUri = "https://coinmate.io/api/";

        public List<string> Pairs { get; }


        public CoinmateLogic()
        {
            Pairs = new List<string>() { "BTC_EUR", "ETH_EUR" };
        }

        public async Task<Order> GetOrderByOrderIdAsync(long orderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeSeconds();

            var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
                new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
                new KeyValuePair<string, string>("nonce", nonce.ToString()),
                new KeyValuePair<string, string>("signature", hashHMACHex),
                new KeyValuePair<string, string>("orderId", orderId.ToString()),

            };

            var content = new FormUrlEncodedContent(pairs);

            var result = await httpClient.PostAsync(baseUri + "orderById", content);

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<GetOrderByIdResponse>(stream);
                return res.data;
            }

        }

        public async Task<List<OrderHistory>> GetOrderHistoryAsync(string currencyPair)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeSeconds();

            var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
                new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
                new KeyValuePair<string, string>("nonce", nonce.ToString()),
                new KeyValuePair<string, string>("signature", hashHMACHex),
                new KeyValuePair<string, string>("currencyPair", currencyPair),

            };

            var content = new FormUrlEncodedContent(pairs);

            var result = await httpClient.PostAsync(baseUri + "orderHistory", content);

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<GetOrderHistoryResponse>(stream);
                return res.data;
            }

        }


        public async Task<CancelOrderResponse> CancelOrderAsync(long orderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeSeconds();

            var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
                new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
                new KeyValuePair<string, string>("nonce", nonce.ToString()),
                new KeyValuePair<string, string>("signature", hashHMACHex),
                new KeyValuePair<string, string>("orderId", orderId.ToString()),
            };

            var content = new FormUrlEncodedContent(pairs);

            var result = await httpClient.PostAsync(baseUri + "cancelOrder", content);

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<CancelOrderResponse>(stream);
                return res;
            }
        }
        
        public async Task<BuyInstantResponse> BuyInstant(string currencyPair, double amountToPayInSecondCurrency)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeSeconds();

            var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
                new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
                new KeyValuePair<string, string>("nonce", nonce.ToString()),
                new KeyValuePair<string, string>("signature", hashHMACHex),
                new KeyValuePair<string, string>("total", amountToPayInSecondCurrency.ToString()),
                new KeyValuePair<string, string>("currencyPair", currencyPair)
            };

            var content = new FormUrlEncodedContent(pairs);

            var result = await httpClient.PostAsync(baseUri + "buyInstant", content);

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<BuyInstantResponse>(stream);
                return res;
            }

        }


        public async Task BuyAsync()
        {

            var nonce = DateTimeOffset.Now.ToUnixTimeSeconds();

            var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
                new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
                new KeyValuePair<string, string>("nonce", nonce.ToString()),
                new KeyValuePair<string, string>("signature", hashHMACHex),
            };

            var content = new FormUrlEncodedContent(pairs);

            var result = await httpClient.PostAsync(baseUri + "tradeHistory", content);






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
            var upair = pair.Replace("_", "");
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

                    var res = await JsonSerializer.DeserializeAsync<CmResult>(ms);

                    if (res.Event == "data" && res.payload != null)
                    {
                        foreach (var x in res.payload.asks)
                        {
                            var dbEntry = InMemDatabase.Instance.Items.SingleOrDefault(p => p.Exch == nameof(Coinmate) && p.Pair == upair && p.amount == x.amount && p.askPrice == x.price);
                            if (dbEntry == null)
                                InMemDatabase.Instance.Items.Add(new DBItem() { Exch = nameof(Coinmate), Pair = upair, amount = x.amount, askPrice = x.price });
                        }


                        foreach (var x in res.payload.bids)
                        {
                            var dbEntry = InMemDatabase.Instance.Items.SingleOrDefault(p => p.Exch == nameof(Coinmate) && p.Pair == upair && p.amount == x.amount && p.bidPrice == x.price);
                            if (dbEntry == null)
                                InMemDatabase.Instance.Items.Add(new DBItem() { Exch = nameof(Coinmate), Pair = upair, amount = x.amount, bidPrice = x.price });
                        }

                        foreach (var w in InMemDatabase.Instance.Items.Where(p => p.Exch == nameof(Coinmate) && p.Pair == upair))
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


}
