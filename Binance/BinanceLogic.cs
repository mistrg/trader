using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Trader;
using System.Linq;
using System.Text.Json;

namespace Trader.Binance
{
    public class BinanceLogic
    {
        public List<string> Pairs { get; }


        public BinanceLogic()
        {
            Pairs = new List<string>() { "BTCEUR", "ETHEUR" };
        }
        static string baseUri = "https://api.binance.com/api/v3/";
        private static readonly HttpClient httpClient = new HttpClient();




        public async Task<OrderResponse> SellMarketAsync(OrderCandidate orderCandidate)
        {

            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() * 1000;

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),

                new KeyValuePair<string, string>("symbol", orderCandidate.Pair),
                new KeyValuePair<string, string>("side", "SELL"),
                new KeyValuePair<string, string>("type", "MARKET"),
                new KeyValuePair<string, string>("quantity", string.Format("{0:0.##############}", orderCandidate.Amount)),
                new KeyValuePair<string, string>("newOrderRespType","RESULT"),
                //new KeyValuePair<string, string>("recvWindow","5000"), 
                new KeyValuePair<string, string>("newClientOrderId",orderCandidate.Id.ToString()), 
                
                 
            };

            var content = new FormUrlEncodedContent(pairs);
            var urlEncodedString = await content.ReadAsStringAsync();

            string hashHMACHex = Cryptography.HashHMACHex(Config.BinanceSecretKey, urlEncodedString);





            pairs.Add(new KeyValuePair<string, string>("signature", hashHMACHex));
            var finalContent = new FormUrlEncodedContent(pairs);



            finalContent.Headers.Add("X-MBX-APIKEY", Config.BinanceApiKey);

            var result = await httpClient.PostAsync(baseUri + "order", finalContent);

            if (!result.IsSuccessStatusCode)
            {
                var str = await result.Content.ReadAsStringAsync();
                Presenter.ShowPanic($"Error HTTP: {result.StatusCode} - {result.ReasonPhrase} - {str}");
            }


            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<OrderResponse>(stream);
                return res;
            }
        }

        public void ListenToOrderbook(CancellationToken stoppingToken)
        {

            foreach (var pair in Pairs)
            {

                var t = new Task(async () =>
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {

                        try
                        {
                            var res = await httpClient.GetFromJsonAsync<BIResult>(baseUri + "depth?symbol=" + pair);

                            foreach (var x in res.asks)
                            {
                                var amount = double.Parse(x[1]);
                                var price = double.Parse(x[0]);
                                var dbEntry = InMemDatabase.Instance.Items.SingleOrDefault(p => p.Exch == nameof(Binance) && p.Pair == pair && p.amount == amount && p.askPrice == price);
                                if (dbEntry == null)
                                    InMemDatabase.Instance.Items.Add(new DBItem() { Exch = nameof(Binance), Pair = pair, amount = amount, askPrice = price });
                            }


                            foreach (var x in res.bids)
                            {
                                var amount = double.Parse(x[1]);
                                var price = double.Parse(x[0]);

                                var dbEntry = InMemDatabase.Instance.Items.SingleOrDefault(p => p.Exch == nameof(Binance) && p.Pair == pair && p.amount == amount && p.bidPrice == price);
                                if (dbEntry == null)
                                    InMemDatabase.Instance.Items.Add(new DBItem() { Exch = nameof(Binance), Pair = pair, amount = amount, bidPrice = price });
                            }

                            foreach (var w in InMemDatabase.Instance.Items.Where(p => p.Exch == nameof(Binance) && p.Pair == pair))
                            {
                                var askItem = res.asks.SingleOrDefault(p => p[1] == w.amount.ToString() && p[0] == w.askPrice.ToString());

                                var bidItem = res.bids.SingleOrDefault(p => p[1] == w.amount.ToString() && p[0] == w.bidPrice.ToString());

                                if (askItem == null && bidItem == null)
                                    w.EndDate = DateTime.Now;

                            }

                            Thread.Sleep(1000);
                        }
                        catch
                        {
                        }
                    }
                }, stoppingToken);

                t.Start();

            }

        }

    }
}