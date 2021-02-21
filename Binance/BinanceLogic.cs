using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Trader;
using System.Linq;
using System.Text.Json;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Trader.Binance
{
    public class BinanceLogic : IExchangeLogic
    {
        public List<string> Pairs { get; }

        private readonly Presenter _presenter;

        public BinanceLogic(Presenter presenter)
        {
            _presenter = presenter;
            Pairs = new List<string>() { "BTCEUR" };
        }
        static string baseUri = "https://api.binance.com/api/v3/";
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<AccountInfo> GetAccountInformationAsync()
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() * 1000;

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),
            };

            var content = new FormUrlEncodedContent(pairs);
            var urlEncodedString = await content.ReadAsStringAsync();

            string hashHMACHex = Cryptography.HashHMACHex(Config.BinanceSecretKey, urlEncodedString);


            pairs.Add(new KeyValuePair<string, string>("signature", hashHMACHex));
            var finalContent = new FormUrlEncodedContent(pairs);


            var url = baseUri + "account?" + await finalContent.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", Config.BinanceApiKey);
            var result = await httpClient.GetAsync(url);



            if (!result.IsSuccessStatusCode)
            {
                var str = await result.Content.ReadAsStringAsync();
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} - {result.ReasonPhrase} - {str}");
            }


            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<AccountInfo>(stream);
                return res;
            }
        }


        public async Task PrintAccountInformationAsync()
        {
            var result = await GetAccountInformationAsync();
            if (result == null || result.balances == null || result.balances.Count == 0)
            {
                _presenter.ShowError("Could not get balances on Binance.");
                return;
            }
            var message = "BI free balances: ";


            foreach (var item in result.balances)
            {
                if (item.freeNum > 0)
                    message += $" {item.freeNum}{item.asset}";
            }

            _presenter.ShowInfo(message);
        }

        public async Task<double> GetAvailableAmountAsync(string currency)
        {
            var biAccount = await GetAccountInformationAsync();
            if (biAccount == null)
            {
                _presenter.ShowError("Binance account info not accessible");
                return 0;
            }
            var item = biAccount.balances.SingleOrDefault(p => p.asset == currency);

            if (item == null)
            {
                _presenter.ShowError("Binance BTC balance not accessible");
                return 0;
            }

            return item.freeNum;

        }


        public async Task<Tuple<bool, SellResult>> SellMarketAsync(OrderCandidate orderCandidate)
        {
            var result = new SellResult();


            if (!Config.ProcessTrades)
            {
                var comment = "SellMarketAsync skipped. ProcessTrades is not activated";
                _presenter.Warning(comment);
                result.Comment = comment;

                return new Tuple<bool, SellResult>(true, result);
            }

            orderCandidate.Amount = Math.Round(orderCandidate.Amount, 6);

            if (orderCandidate.Amount <= 0)
            {
                var comment = "SellMarketAsync skipped. Amount too small";
                _presenter.ShowError(comment);
                result.Comment = comment;

                return new Tuple<bool, SellResult>(true, result);
            }


            _presenter.ShowInfo("Let's sell");

            OrderResponse sellResponse = null;
            try
            {
                sellResponse = await SellMarketAsync(orderCandidate.Pair, orderCandidate.Amount, orderCandidate.Id);
                result.Status = sellResponse.status;

            }
            catch (System.Exception ex)
            {
                var comment = $"SellMarketAsync failed. {ex}. Please check binance manually.";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);

            }

            if (result == null)
            {
                var comment = $"SellMarketAsync failed. Result is null.";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);
            }


            _presenter.ShowInfo($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} {result.side} {result.type} OrderId {result.orderId} OCID {result.clientOrderId}  price: {result.price} symbol: {result.symbol} Qty: {result.executedQty}/{result.origQty} cumQty: {result.cummulativeQuoteQty}");

            if (result.Status == "FILLED")
                _presenter.ShowInfo("Successfully sold");
            else
            {
                _presenter.ShowPanic("Check line above for problems");
            }

            return new Tuple<bool, SellResult>(result.status == "FILLED", result);
        }


        private async Task<OrderResponse> SellMarketAsync(string currencyPair, double amount, long clientOrderId)
        {

            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() * 1000;

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),

                new KeyValuePair<string, string>("symbol", currencyPair),
                new KeyValuePair<string, string>("side", "SELL"),
                new KeyValuePair<string, string>("type", "MARKET"),
                new KeyValuePair<string, string>("quantity", string.Format("{0:0.##############}", amount)),
                new KeyValuePair<string, string>("newOrderRespType","FULL"),
                //new KeyValuePair<string, string>("recvWindow","5000"), 
                new KeyValuePair<string, string>("newClientOrderId",clientOrderId.ToString()),


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
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} - {result.ReasonPhrase} - {str}");
            }


            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<OrderResponse>(stream);
                return res;
            }
        }



        private async Task<OrderResponse> BuyLimitOrderAsync(OrderCandidate orderCandidate)
        {

            var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds() * 1000;

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),

                new KeyValuePair<string, string>("symbol", orderCandidate.Pair),
                new KeyValuePair<string, string>("side", "BUY"),
                new KeyValuePair<string, string>("quantity", string.Format("{0:0.##############}", orderCandidate.Amount)),
                new KeyValuePair<string, string>("price", string.Format("{0:0.##############}", orderCandidate.UnitAskPrice)),
                new KeyValuePair<string, string>("timeInForce", "GTC"),


                new KeyValuePair<string, string>("newOrderRespType","FULL"),
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
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} - {result.ReasonPhrase} - {str}");
            }


            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<OrderResponse>(stream);
                return res;
            }
        }



        public async Task<List<DBItem>> GetOrderBookAsync(string pair)
        {
            var result = new List<DBItem>();


            try
            {
                var res = await httpClient.GetFromJsonAsync<BIResult>(baseUri + "depth?symbol=" + pair);

                if (res == null)
                    return result;


                foreach (var x in res.asks)
                {
                    var amount = double.Parse(x[1]);
                    var price = double.Parse(x[0]);
                    result.Add(new DBItem() { Exch = nameof(Binance), Pair = pair, amount = amount, askPrice = price });
                }


                foreach (var x in res.bids)
                {
                    var amount = double.Parse(x[1]);
                    var price = double.Parse(x[0]);

                    result.Add(new DBItem() { Exch = nameof(Binance), Pair = pair, amount = amount, bidPrice = price });

                }
            }
            catch
            {
                throw;
            }
            return result;

        }
        private void ListenToOrderbook(CancellationToken stoppingToken)
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
                        }
                        catch
                        {
                        }


                    }
                }, stoppingToken);

                t.Start();

            }

        }

        public double GetTradingTakerFeeRate()
        {
            return 0.001;

        }

        Task<Tuple<bool, BuyResult>> IExchangeLogic.BuyLimitOrderAsync(OrderCandidate orderCandidate)
        {
            throw new Exception();
        }

    }
}