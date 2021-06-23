using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Trader.Exchanges;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Trader.Coinmate
{

    public class CoinmateLogic : BaseExchange, IExchangeLogic
    {

        private static readonly HttpClient httpClient = new HttpClient();

        private readonly Presenter _presenter;

        private string baseUri = "https://coinmate.io/api/";

        private DateTime lastApiCall = DateTime.Now.AddDays(-1);
        public List<string> Pairs { get; }


        public CoinmateLogic(Presenter presenter, ObserverContext context)
                : base(context)
        {
            Pairs = new List<string>() { "BTC_EUR" };
            _presenter = presenter;
        }
        public string GetLongPair(string shortPair)
        {
            if (shortPair.Length == 6)
            {
                return shortPair.Insert(3, "_");
            }
            return shortPair;
        }


        private void Throtle()
        {
            var now = DateTime.Now;
            var sinceLastApiCall = (now - lastApiCall).TotalMilliseconds;

            if (sinceLastApiCall < 650)
            {
                var x = 650 - (int)sinceLastApiCall;
                Thread.Sleep(x);
            }

            lastApiCall = DateTime.Now;
        }

        public async Task<BalanceResponse> GetBalancesAsync()
        {

            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),

            // };
            var pairs = new List<KeyValuePair<string, string>>();
            var content = new FormUrlEncodedContent(pairs);

            Throtle();

            var result = await httpClient.PostAsync(baseUri + "balances", content);

            if (!result.IsSuccessStatusCode)
            {
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");

            }

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<BalanceResponse>(stream);
                return res;
            }

        }

        public async Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            var cmAccount = await GetBalancesAsync();
            if (cmAccount?.data == null)
            {
                _presenter.ShowError("Coinmate account info not accessible");
                return new Tuple<double?, double?>(null, null);
            }

            var btc = cmAccount.data.SingleOrDefault(p => p.Key == currencyPair.Substring(0, 3));
            var euro = cmAccount.data.SingleOrDefault(p => p.Key == currencyPair.Substring(3, 3));

            return new Tuple<double?, double?>(btc.Value?.available, euro.Value?.available);
        }

        public async Task PrintAccountInformationAsync()
        {


            var result = await GetBalancesAsync();
            if (result == null || result.data == null)
            {
                _presenter.ShowError("Could not get balances on Coinmate." + result.errorMessage);
                return;
            }
            var message = "CM balances/available: ";



            foreach (var item in result.data)
            {
                if (item.Value?.balance > 0 || item.Value?.available > 0)
                    message += $" {item.Value.balance}/{item.Value.available}{item.Value.currency}";
            }

            _presenter.ShowInfo(message);
        }

        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            OrderBookTotalCount++;
            var pair = "BTC_EUR";

            var result = new List<DBItem>();
            var upair = pair.Replace("_", "");
            try
            {
                Throtle();

                var res = await httpClient.GetFromJsonAsync<OrderBookResponse>($"{baseUri}orderBook?currencyPair={pair}&groupByPriceLimit=False");

                if (res?.data == null)
                    return result;
                var fee = await GetTradingTakerFeeRateAsync();
                foreach (var x in res.data.asks)
                {
                    result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Coinmate), Pair = upair, amount = x.amount, askPrice = x.price });
                }

                foreach (var x in res.data.bids)
                {
                    result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Coinmate), Pair = upair, amount = x.amount, bidPrice = x.price });
                }
            }
            catch
            {
                OrderBookFailCount++;
            }
            if (result.Count > 0)
                OrderBookSuccessCount++;
            return result;
        }
        public async Task<Order> GetOrderByOrderIdAsync(string orderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),
            //     new KeyValuePair<string, string>("orderId", orderId),

            // };
            var pairs = new List<KeyValuePair<string, string>>();

            var content = new FormUrlEncodedContent(pairs);

            Throtle();

            var result = await httpClient.PostAsync(baseUri + "orderById", content);

            if (!result.IsSuccessStatusCode)
            {
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");

                _presenter.ShowInfo(await result.Content.ReadAsStringAsync());
            }

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<GetOrderByIdResponse>(stream);
                return res.data;
            }

        }

        public async Task<TradeHistoryResponse> GetTradeHistoryAsync(string? orderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),

            // };
            var pairs = new List<KeyValuePair<string, string>>();

            if (orderId != null)
                pairs.Add(new KeyValuePair<string, string>("orderId", orderId.ToString()));

            var content = new FormUrlEncodedContent(pairs);

            Throtle();

            var result = await httpClient.PostAsync(baseUri + "tradeHistory", content);

            if (!result.IsSuccessStatusCode)

                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");

            var resa = await result.Content.ReadAsStringAsync();
            _presenter.ShowInfo(resa);

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<TradeHistoryResponse>(stream);

                //     foreach (var item in res.data)
                // {
                //     Console.WriteLine($"transactionType: {item.transactionType} orderId:{item.orderId} fee:{item.fee} feeCurrency:{item.feeCurrency} amount:{item.amount} amountCurrency:{item.amountCurrency} price:{item.price} priceCurrency:{item.priceCurrency}");
                // }

                return res;

            }



        }
        public async Task<TransactionHistoryResponse> GetTransactionHistoryAsync(long? orderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),

            // };
            var pairs = new List<KeyValuePair<string, string>>();

            if (orderId != null)
                pairs.Add(new KeyValuePair<string, string>("orderId", orderId.ToString()));

            var content = new FormUrlEncodedContent(pairs);

            Throtle();

            var result = await httpClient.PostAsync(baseUri + "transactionHistory", content);

            if (!result.IsSuccessStatusCode)
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");

            // var resa = await result.Content.ReadAsStringAsync();
            // _presenter.ShowInfo(resa);

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<TransactionHistoryResponse>(stream);

                //     foreach (var item in res.data)
                // {
                //     Console.WriteLine($"transactionType: {item.transactionType} orderId:{item.orderId} fee:{item.fee} feeCurrency:{item.feeCurrency} amount:{item.amount} amountCurrency:{item.amountCurrency} price:{item.price} priceCurrency:{item.priceCurrency}");
                // }

                return res;

            }

        }


        public async Task<List<OrderHistory>> GetOrderHistoryAsync(string currencyPair)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),
            //     new KeyValuePair<string, string>("currencyPair", currencyPair),

            // };
            var pairs = new List<KeyValuePair<string, string>>();

            var content = new FormUrlEncodedContent(pairs);

            Throtle();

            var result = await httpClient.PostAsync(baseUri + "orderHistory", content);

            if (!result.IsSuccessStatusCode)
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");

            var resa = await result.Content.ReadAsStringAsync();
            _presenter.ShowInfo(resa);
            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<GetOrderHistoryResponse>(stream);
                return res.data;
            }

        }


        public async Task<CancelOrderResponse> CancelOrderAsync(long orderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),
            //     new KeyValuePair<string, string>("orderId", orderId.ToString()),
            // };

            var pairs = new List<KeyValuePair<string, string>>();

            var content = new FormUrlEncodedContent(pairs);
            Throtle();

            var result = await httpClient.PostAsync(baseUri + "cancelOrder", content);

            if (!result.IsSuccessStatusCode)
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");


            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<CancelOrderResponse>(stream);
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
            _presenter.ShowInfo($"Let's buy");

            _presenter.PrintOrderCandidate(orderCandidate);

            BuyResponse buyResponse = null;
            try
            {
                var pair = GetLongPair(orderCandidate.Pair);
                if (!Pairs.Any(p => p == pair))
                {
                    var comment = "Unsupported currency pair. Process cancel...";
                    _presenter.ShowError(comment);

                    result.Comment = comment;

                    return new Tuple<bool, BuyResult>(false, result);
                }

                buyResponse = await BuyLimitOrderAsync(pair, orderCandidate.Amount, orderCandidate.UnitAskPrice, orderCandidate.Id);
            }
            catch (System.Exception ex)
            {
                var comment = $"Buylimit failed. {ex}. Process cancel...";

                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, BuyResult>(false, result);
            }

            if (buyResponse == null)
            {
                var comment = $"Buyresponse is empty. Process cancel...";
                _presenter.ShowError(comment);
                result.Comment = comment;
                return new Tuple<bool, BuyResult>(false, result);
            }

            if (buyResponse.error)
            {
                var comment = $"Buylimit failed. {buyResponse.errorMessage}. Process cancel...";
                _presenter.ShowError(comment);
                result.Comment = comment;
                return new Tuple<bool, BuyResult>(false, result);
            }

            if (buyResponse.data == null || buyResponse.data <= 0)
            {
                var comment = $"Buylimit failed. Invalid order ID. Process cancel...";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, BuyResult>(false, result);
            }

            _presenter.ShowInfo($"Waiting for buy confirmation");
            result.OrderId = buyResponse.data.Value.ToString();

            Order response = null;

            bool opComplete = System.Threading.SpinWait.SpinUntil(() =>
            {
                try
                {
                    response = GetOrderByOrderIdAsync(result.OrderId).Result;
                    result.Status = response.status;
                    result.Timestamp = response.timestamp;



                    result.OriginalAmount = response.originalAmount;
                    result.RemainingAmount = response.remainingAmount;
                }
                catch (Exception ex)
                {
                    _presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
                }
                return response != null && (response.status == "FILLED" || response.status == "PARTIALLY_FILLED" || response.status == "CANCELLED");


            }, TimeSpan.FromMilliseconds(Config.BuyTimeoutInMs));

            var buySuccess = result.Status != null && (result.Status == "FILLED" || result.Status == "PARTIALLY_FILLED");
            if (!buySuccess)
            {
                _presenter.ShowInfo($"Buylimit order was sent but could not be confirmed in time. Current state is {result?.Status} Trying to cancel the order.");

                if (result?.Status == "CANCELLED")
                {
                    if (result?.RemainingAmount == result?.OriginalAmount)
                    {
                        var comment = "Order was already cancelled successfully.Process cancel...";
                        _presenter.ShowInfo(comment);
                        result.Comment = comment;
                        return new Tuple<bool, BuyResult>(false, result);
                    }
                    else
                    {
                        orderCandidate.Amount -= result.RemainingAmount.Value;
                        await UpdateBuyResult(result);

                        var comment = $"Order is cancelled but there is an open position, that we need to sell. Amount adjusted  to {orderCandidate.Amount}.";
                        _presenter.ShowInfo(comment);
                        result.Comment = comment;
                        return new Tuple<bool, BuyResult>(true, result);
                    }
                }

                //OPENED state
                CancelOrderResponse buyCancelResult = null;
                try
                {
                    buyCancelResult = await CancelOrderAsync(buyResponse.data.Value);
                }
                catch (System.Exception ex)
                {
                    _presenter.ShowPanic($"CancelOrderAsync failed. {ex}. Maybe buy was successful.");
                }

                if (buyCancelResult != null && buyCancelResult.data)
                {
                    var comment = "Order was cancelled successfully.Process cancel...";
                    _presenter.ShowInfo(comment);

                    result.Comment = comment;
                    return new Tuple<bool, BuyResult>(false, result);
                }
                else
                {
                    var comment = $"CancelOrderAsync  exited with wrong errorcode. Assuming the trade was finished.";
                    _presenter.ShowPanic(comment);
                    result.Comment = comment;
                    return new Tuple<bool, BuyResult>(false, result);
                }
            }

            if (result.Status == "PARTIALLY_FILLED")
            {
                if (result.RemainingAmount is null)
                {
                    var comment = $"Partial buy was done but remaing amount is not set. Don't know how much to sell. Please check the coinmate platform manually...Process cancel...";
                    _presenter.ShowPanic(comment);
                    result.Comment = comment;
                    return new Tuple<bool, BuyResult>(false, result);
                }

                orderCandidate.Amount -= result.RemainingAmount.Value;

            }


            await UpdateBuyResult(result);

            _presenter.ShowInfo($"Buy successful");

            return new Tuple<bool, BuyResult>(true, result);

        }


        private async Task UpdateBuyResult(BuyResult result)
        {
            //Check the fees
            try
            {
                var th = await GetTradeHistoryAsync(result.OrderId);
                if (th != null && th.data != null && th.data.Count > 0)
                {
                    result.CummulativeFee = th.data.Sum(p => p.price != 0 ? p.fee / p.price : 0);
                    result.CummulativeFeeQuote = th.data.Sum(p => p.fee);
                    result.CummulativeQuoteQty = th.data.Sum(p => p.price * p.amount);

                }
            }
            catch (System.Exception ex)
            {
                var comment = $"Can not determine fees. " + ex;
                _presenter.ShowError(comment);
                result.Comment = comment;


            }
        }



        private async Task<BuyResponse> BuyLimitOrderAsync(string currencyPair, double amount, double price, long clientOrderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),
            //     new KeyValuePair<string, string>("amount", string.Format("{0:0.##############}", amount)),
            //     new KeyValuePair<string, string>("price", string.Format("{0:0.##############}", price)),
            //     new KeyValuePair<string, string>("currencyPair", currencyPair),
            //     new KeyValuePair<string, string>("immediateOrCancel", 1.ToString()),
            //     new KeyValuePair<string, string>("clientOrderId", clientOrderId.ToString()),

            // };

            var pairs = new List<KeyValuePair<string, string>>();

            var content = new FormUrlEncodedContent(pairs);
            Throtle();

            var result = await httpClient.PostAsync(baseUri + "buyLimit", content);

            if (!result.IsSuccessStatusCode)
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<BuyResponse>(stream);
                return res;
            }
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

            SellInstantOrderResponse sellResponse = null;
            try
            {
                var pair = GetLongPair(orderCandidate.Pair);
                if (!Pairs.Any(p => p == pair))
                {
                    var comment = "Unsupported currency pair. Process cancel...";
                    _presenter.ShowError(comment);

                    result.Comment = comment;

                    return new Tuple<bool, SellResult>(false, result);
                }

                sellResponse = await SellMarketAsync(pair, orderCandidate.Amount, orderCandidate.Id);
            }
            catch (System.Exception ex)
            {
                var comment = $"SellMarketAsync failed. {ex}. Process cancel...";

                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);
            }

            if (sellResponse == null)
            {
                var comment = $"sellResponse is empty. Process cancel...";
                _presenter.ShowError(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);
            }

            if (sellResponse.error)
            {
                var comment = $"SellMarketAsync failed. {sellResponse.errorMessage}. Process cancel...";
                _presenter.ShowError(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);
            }

            if (sellResponse.data == null || sellResponse.data <= 0)
            {
                var comment = $"SellMarketAsync failed. Invalid order ID. Process cancel...";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);
            }

            _presenter.ShowInfo($"Waiting for sell confirmation");
            result.OrderId = sellResponse.data.Value.ToString();

            Order response = null;

            bool opComplete = System.Threading.SpinWait.SpinUntil(() =>
            {
                try
                {

                    response = GetOrderByOrderIdAsync(result.OrderId).Result;
                    result.Status = response.status;
                    result.RemainingAmount = response.remainingAmount;
                    result.OriginalAmount = response.originalAmount;
                    result.Timestamp = response.timestamp;

                }
                catch (Exception ex)
                {
                    _presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
                }
                return response != null && (response.status == "FILLED" || response.status == "PARTIALLY_FILLED" || response.status == "CANCELLED");


            }, TimeSpan.FromMilliseconds(Config.SellTimeoutInMs));

            var sellSuccess = result.Status != null && (result.Status == "FILLED");

            if (!sellSuccess)
            {
                var comment = $"Sell was not succeful. {orderCandidate.SellExchange} OrderId: {result.OrderId} Status: {result.Status} Please check the coinmate platform manually...Process cancel...";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, SellResult>(false, result);
            }


            try
            {

                var th = await GetTradeHistoryAsync(result.OrderId);
                if (th != null && th.data != null && th.data.Count > 0)
                {
                    result.CummulativeFee = th.data.Sum(p => p.price != 0 ? p.fee / p.price : 0);
                    result.CummulativeFeeQuote = th.data.Sum(p => p.fee);
                    result.CummulativeQuoteQty = th.data.Sum(p => p.price * p.amount);

                }
            }
            catch (System.Exception ex)
            {
                var comment = $"Can not determine fees. " + ex;
                _presenter.ShowError(comment);
                result.Comment = comment;

            }

            _presenter.ShowInfo($"Sell successful");


            return new Tuple<bool, SellResult>(true, result);


        }

        private async Task<SellInstantOrderResponse> SellMarketAsync(string currencyPair, double amount, long clientOrderId)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),

            //     new KeyValuePair<string, string>("amount", string.Format("{0:0.##############}", amount)),
            //     new KeyValuePair<string, string>("currencyPair", currencyPair),
            //     new KeyValuePair<string, string>("clientOrderId", clientOrderId.ToString()),

            // };
            var pairs = new List<KeyValuePair<string, string>>();

            var content = new FormUrlEncodedContent(pairs);

            Throtle();

            var result = await httpClient.PostAsync(baseUri + "sellInstant", content);

            if (!result.IsSuccessStatusCode)
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<SellInstantOrderResponse>(stream);
                return res;
            }
        }


        public async Task<double> GetTradingTakerFeeRateAsync()
        {
            return 0.0023;
        }

        private async Task<BuyResponse> BuyInstant(string currencyPair, double amountToPayInSecondCurrency)
        {
            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            // var signatureInput = nonce + Config.CoinmateClientId + Config.CoinmatePublicKey;

            // string hashHMACHex = Cryptography.HashHMACHex(Config.CoinmatePrivateKey, signatureInput);

            // var pairs = new List<KeyValuePair<string, string>>
            // {
            //     new KeyValuePair<string, string>("clientId", Config.CoinmateClientId),
            //     new KeyValuePair<string, string>("publicKey", Config.CoinmatePublicKey),
            //     new KeyValuePair<string, string>("nonce", nonce.ToString()),
            //     new KeyValuePair<string, string>("signature", hashHMACHex),
            //     new KeyValuePair<string, string>("total", string.Format("{0:0.##############}", amountToPayInSecondCurrency)),
            //     new KeyValuePair<string, string>("currencyPair", currencyPair)
            // };
            var pairs = new List<KeyValuePair<string, string>>();

            var content = new FormUrlEncodedContent(pairs);

            Throtle();

            var result = await httpClient.PostAsync(baseUri + "buyInstant", content);
            if (!result.IsSuccessStatusCode)
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");


            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<BuyResponse>(stream);
                return res;
            }

        }

        public Task<bool> BuyLimitOrderAsync(Arbitrage arbitrage)
        {
            throw new System.Exception();
        }

        public Task<bool> SellMarketAsync(Arbitrage orderCandidate)
        {
            throw new System.Exception();
        }
    }


}
