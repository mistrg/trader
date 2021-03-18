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
using System.Diagnostics;

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
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

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


        public async Task GetMyTrades(string currencyPair)
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),
                new KeyValuePair<string, string>("symbol", currencyPair),
            };

            var content = new FormUrlEncodedContent(pairs);
            var urlEncodedString = await content.ReadAsStringAsync();

            string hashHMACHex = Cryptography.HashHMACHex(Config.BinanceSecretKey, urlEncodedString);


            pairs.Add(new KeyValuePair<string, string>("signature", hashHMACHex));
            var finalContent = new FormUrlEncodedContent(pairs);


            var url = $"{baseUri}myTrades?" + await finalContent.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", Config.BinanceApiKey);
            var result = await httpClient.GetAsync(url);



            if (!result.IsSuccessStatusCode)
            {
                var str = await result.Content.ReadAsStringAsync();
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} - {result.ReasonPhrase} - {str}");
            }

            var stri = await result.Content.ReadAsStringAsync();
            _presenter.ShowInfo(stri);
            // using (var stream = await result.Content.ReadAsStreamAsync())
            // {
            //     var res = await JsonSerializer.DeserializeAsync<AccountInfo>(stream);
            //     return res;
            // }
        }


        public async Task GetAllOrders(string currencyPair)
        {
            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),
                new KeyValuePair<string, string>("symbol", currencyPair),
            };

            var content = new FormUrlEncodedContent(pairs);
            var urlEncodedString = await content.ReadAsStringAsync();

            string hashHMACHex = Cryptography.HashHMACHex(Config.BinanceSecretKey, urlEncodedString);


            pairs.Add(new KeyValuePair<string, string>("signature", hashHMACHex));
            var finalContent = new FormUrlEncodedContent(pairs);


            var url = $"{baseUri}allOrders?" + await finalContent.ReadAsStringAsync();

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("X-MBX-APIKEY", Config.BinanceApiKey);
            var result = await httpClient.GetAsync(url);



            if (!result.IsSuccessStatusCode)
            {
                var str = await result.Content.ReadAsStringAsync();
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} - {result.ReasonPhrase} - {str}");
            }

            var stri = await result.Content.ReadAsStringAsync();
            _presenter.ShowInfo(stri);
            // using (var stream = await result.Content.ReadAsStreamAsync())
            // {
            //     var res = await JsonSerializer.DeserializeAsync<AccountInfo>(stream);
            //     return res;
            // }
        }



        public async Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            var biAccount = await GetAccountInformationAsync();
            if (biAccount == null)
            {
                _presenter.ShowError("Binance account info not accessible");
                return new Tuple<double?, double?>(null, null);
            }
            var btc = biAccount.balances.SingleOrDefault(p => p.asset == currencyPair.Substring(0, 3))?.freeNum;
            var euro = biAccount.balances.SingleOrDefault(p => p.asset == currencyPair.Substring(3, 3))?.freeNum;

            return new Tuple<double?, double?>(btc, euro); ;
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
            _presenter.ShowInfo($"Let's sell");
            _presenter.PrintOrderCandidate(orderCandidate);

            orderCandidate.Amount = Math.Round(orderCandidate.Amount, 6);

            if (orderCandidate.Amount <= 0)
            {
                var comment = "SellMarketAsync skipped. Amount too small";
                _presenter.ShowError(comment);
                result.Comment = comment;

                return new Tuple<bool, SellResult>(false, result);
            }




            OrderResponse sellResponse = null;
            try
            {
                sellResponse = await SellMarketAsync(orderCandidate.Pair, orderCandidate.Amount, orderCandidate.Id);
                result.Status = sellResponse.status;
                result.OrderId = sellResponse.orderId;
                result.OriginalAmount = sellResponse.origQtyNum;
                result.RemainingAmount = sellResponse.origQtyNum - sellResponse.executedQtyNum;
                result.CummulativeFee = sellResponse.CummulativeFee ?? 0;
                result.CummulativeFeeQuote = sellResponse.CummulativeFeeQuote ?? 0;
                result.CummulativeQuoteQty = sellResponse.cummulativeQuoteQtyNum;
                result.Timestamp = sellResponse.transactTime;

            }
            catch (System.Exception ex)
            {
                var comment = $"SellMarketAsync failed. {ex}. Please check binance manually.";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);

            }

            if (sellResponse == null)
            {
                var comment = $"SellMarketAsync failed. Result is null.";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);
            }


            if (result.Status == "FILLED")
                _presenter.ShowInfo($"Sell successful");
            else
                _presenter.ShowInfo($"Sell result not clear");


            return new Tuple<bool, SellResult>(result.Status == "FILLED", result);
        }


        private async Task<OrderResponse> SellMarketAsync(string currencyPair, double amount, long clientOrderId)
        {

            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

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



        private async Task<OrderResponse> SellOcoAsync(string currencyPair, double amount, double price, double stopPrice, long clientOrderId)
        {

            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),

                new KeyValuePair<string, string>("symbol", currencyPair),
                new KeyValuePair<string, string>("side", "SELL"),
                new KeyValuePair<string, string>("quantity", string.Format("{0:0.##############}", amount)),
                new KeyValuePair<string, string>("price", string.Format("{0:0.##############}", price)),
                new KeyValuePair<string, string>("stopPrice", string.Format("{0:0.##############}", stopPrice)),

                new KeyValuePair<string, string>("stopLimitTimeInForce","GTC"),
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

            var result = await httpClient.PostAsync(baseUri + "oco", finalContent);

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


        public async Task<Tuple<bool, BuyResult>> BuyLimitOrderAsync(OrderCandidate orderCandidate)
        {
            var result = new BuyResult();


            if (!Config.ProcessTrades)
            {
                var comment = "BuyLimitOrderAsync skipped. ProcessTrades is not activated";
                _presenter.Warning(comment);
                result.Comment = comment;

                return new Tuple<bool, BuyResult>(true, result);
            }


            orderCandidate.Amount = Math.Round(orderCandidate.Amount, 6);
            _presenter.ShowInfo("Let's buy");
            _presenter.PrintOrderCandidate(orderCandidate);

            if (orderCandidate.Amount <= 0)
            {
                result.Comment = "BuyLimitOrderAsync skipped. Amount too small";
                _presenter.ShowError(result.Comment);

                return new Tuple<bool, BuyResult>(false, result);
            }



            OrderResponse buyResponse = null;
            try
            {
                buyResponse = await BuyLimitOrderAsync(orderCandidate.Pair, orderCandidate.Amount, orderCandidate.UnitAskPrice, orderCandidate.Id);

                result.Status = buyResponse.status;
                result.OrderId = buyResponse.orderId;
                result.OriginalAmount = buyResponse.origQtyNum;
                result.RemainingAmount = buyResponse.origQtyNum - buyResponse.executedQtyNum;
                result.CummulativeFee = buyResponse.CummulativeFee ?? 0;
                result.CummulativeFeeQuote = buyResponse.CummulativeFeeQuote ?? 0;
                result.CummulativeQuoteQty = buyResponse.cummulativeQuoteQtyNum;
                result.Price = buyResponse.priceNum;
                result.Timestamp = buyResponse.transactTime;
            }
            catch (System.Exception ex)
            {
                result.Comment = $"BuyLimitOrderAsync failed. {ex}. Please check binance manually.";

                _presenter.ShowPanic(result.Comment);

                return new Tuple<bool, BuyResult>(false, result);

            }

            if (result == null)
            {
                result.Comment = $"BuyLimitOrderAsync failed. Result is null.";
                _presenter.ShowError(result.Comment);
                return new Tuple<bool, BuyResult>(false, result);
            }


            orderCandidate.Amount = (result.OriginalAmount ?? 0) - result.RemainingAmount.Value - result.CummulativeFee;


            var boughtSomething = Math.Round(orderCandidate.Amount, 6) > 0;

            if (boughtSomething)
                _presenter.ShowInfo("Buy successful");
            else
                _presenter.ShowInfo($"Buy not done");



            return new Tuple<bool, BuyResult>(boughtSomething, result);
        }


        private async Task<OrderResponse> BuyLimitOrderAsync(string pair, double amount, double price, long ocid)
        {

            var timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("timestamp", timestamp.ToString()),

                new KeyValuePair<string, string>("symbol", pair),
                new KeyValuePair<string, string>("side", "BUY"),
                new KeyValuePair<string, string>("type", "LIMIT"),
                new KeyValuePair<string, string>("quantity", string.Format("{0:0.##############}", amount)),
                new KeyValuePair<string, string>("price", string.Format("{0:0.##############}", price)),
                new KeyValuePair<string, string>("timeInForce", "IOC"),


                new KeyValuePair<string, string>("newOrderRespType","FULL"),
                //new KeyValuePair<string, string>("recvWindow","5000"), 
                new KeyValuePair<string, string>("newClientOrderId",ocid.ToString()),


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





        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            var pair = "BTCEUR";
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
                    result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Binance), Pair = pair, amount = amount, askPrice = price });
                }


                foreach (var x in res.bids)
                {
                    var amount = double.Parse(x[1]);
                    var price = double.Parse(x[0]);

                    result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Binance), Pair = pair, amount = amount, bidPrice = price });

                }
            }
            catch (System.Exception ex)
            {
                //Debug.Write(this); 
            }
            return result;

        }
        // private void ListenToOrderbook(CancellationToken stoppingToken)
        // {

        //     foreach (var pair in Pairs)
        //     {

        //         var t = new Task(async () =>
        //         {
        //             while (!stoppingToken.IsCancellationRequested)
        //             {
        //                 try
        //                 {
        //                     var res = await httpClient.GetFromJsonAsync<BIResult>(baseUri + "depth?symbol=" + pair);

        //                     foreach (var x in res.asks)
        //                     {
        //                         var amount = double.Parse(x[1]);
        //                         var price = double.Parse(x[0]);
        //                         var dbEntry = InMemDatabase.Instance.Items.SingleOrDefault(p => p.Exch == nameof(Binance) && p.Pair == pair && p.amount == amount && p.askPrice == price);
        //                         if (dbEntry == null)
        //                             InMemDatabase.Instance.Items.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Binance), Pair = pair, amount = amount, askPrice = price });
        //                     }


        //                     foreach (var x in res.bids)
        //                     {
        //                         var amount = double.Parse(x[1]);
        //                         var price = double.Parse(x[0]);

        //                         var dbEntry = InMemDatabase.Instance.Items.SingleOrDefault(p => p.Exch == nameof(Binance) && p.Pair == pair && p.amount == amount && p.bidPrice == price);
        //                         if (dbEntry == null)
        //                             InMemDatabase.Instance.Items.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Binance), Pair = pair, amount = amount, bidPrice = price });
        //                     }

        //                     foreach (var w in InMemDatabase.Instance.Items.Where(p => p.Exch == nameof(Binance) && p.Pair == pair))
        //                     {
        //                         var askItem = res.asks.SingleOrDefault(p => p[1] == w.amount.ToString() && p[0] == w.askPrice.ToString());

        //                         var bidItem = res.bids.SingleOrDefault(p => p[1] == w.amount.ToString() && p[0] == w.bidPrice.ToString());

        //                         if (askItem == null && bidItem == null)
        //                             w.EndDate = DateTime.Now;

        //                     }
        //                 }
        //                 catch
        //                 {
        //                 }


        //             }
        //         }, stoppingToken);

        //         t.Start();

        //     }

        // }

        public double GetTradingTakerFeeRate()
        {
            return 0.001;

        }

    }
}