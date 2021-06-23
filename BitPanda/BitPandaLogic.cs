using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Trader.Exchanges;
using Trader.Infrastructure;
using Trader.PostgresDb;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace Trader.BitPanda
{
    public class BitPandaLogic : BaseExchange, IExchangeLogic
    {
        public List<string> Pairs { get; }
        const string pair = "BTC_EUR";
        private string baseUrl = "https://api.exchange.bitpanda.com/public/v1";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly Presenter _presenter;
        private readonly KeyVaultCache _keyVaultCache;


        public BitPandaLogic(Presenter presenter, KeyVaultCache keyVaultCache, ObserverContext context)
                : base(context)
        {
            _keyVaultCache = keyVaultCache;
            _presenter = presenter;
            Pairs = new List<string>() { pair };
        }

        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            OrderBookTotalCount++;
            var upair = pair.Replace("_", "");

            var result = new List<DBItem>();
            try
            {
                using (HttpClient httpClient = GetHttpClient())
                {
                    var response = await httpClient.GetAsync(baseUrl + "/order-book/" + pair + "?depth=100");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<OrderBookResponse>(stream);

                            var fee = await GetTradingTakerFeeRateAsync();
                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(BitPanda), Pair = upair, amount = double.Parse(item.amount), askPrice = double.Parse(item.price) });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(BitPanda), Pair = upair, amount = double.Parse(item.amount), bidPrice = double.Parse(item.price) });
                        }

                    }
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

        public async Task<double> GetTradingTakerFeeRateAsync()
        {
            //return 0.0015;
            if (WhenTradingFeeLastCheck == null || WhenTradingFeeLastCheck < DateTime.Now.AddDays(1))
            {
                try
                {
                    var privateApiKey = _keyVaultCache.GetCachedSecret(nameof(BitPanda) + "ApiSecret");

                    var urlPath = "/account/fees";
                    httpClient.DefaultRequestHeaders.Clear();

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", privateApiKey);



                    var response = await httpClient.GetAsync(baseUrl + urlPath);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(await response.Content.ReadAsStringAsync());
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<TradingFeeResponse>(stream);
                            WhenTradingFeeLastCheck = DateTime.Now;

                            TradingFee = double.Parse(res.active_fee_tier.taker_fee);

                        }


                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }


            }
            if (TradingFee == 0)
                throw new Exception("Invalid trading fee");

            return TradingFee;

        }

        public async Task<CloseOrderByOrderIdResponse> CloseOrderByOrderIdAsync(string orderId)
        {
            CloseOrderByOrderIdResponse result = null;
            var privateApiKey = _keyVaultCache.GetCachedSecret(nameof(BitPanda) + "ApiSecret");
            var urlPath = "/account/orders/" + orderId;

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", privateApiKey);


            var response = await httpClient.DeleteAsync(baseUrl + urlPath);
            if (!response.IsSuccessStatusCode && !string.IsNullOrWhiteSpace(response?.ReasonPhrase))
                result = new CloseOrderByOrderIdResponse() { error = "maybe executed already " + response.ReasonPhrase };

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(await response.Content.ReadAsStringAsync());

                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    result = await JsonSerializer.DeserializeAsync<CloseOrderByOrderIdResponse>(stream);

                }
            }
            return result;
        }

        public async Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            double btcBalance = 0;
            double euroBalance = 0;

            try
            {
                var privateApiKey = _keyVaultCache.GetCachedSecret(nameof(BitPanda) + "ApiSecret");
                var urlPath = "/account/balances";

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", privateApiKey);


                var response = await httpClient.GetAsync(baseUrl + urlPath);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var res = await JsonSerializer.DeserializeAsync<BalancesResponse>(stream);
                        var btcBalances = res.balances.Where(x => x.currency_code == currencyPair.Substring(0, 3)).Select(p => p.available);
                        foreach (var item in btcBalances)
                            btcBalance += double.Parse(item);

                        var eurBalances = res.balances.Where(x => x.currency_code == currencyPair.Substring(3, 3)).Select(p => p.available);
                        foreach (var item in eurBalances)
                            euroBalance += double.Parse(item);
                    }
                }
            }
            catch
            {
                throw new Exception("Counldn't get the balance");
            }

            return new Tuple<double?, double?>(btcBalance, euroBalance);

        }
        public async Task<OrderResponse> GetOrderByOrderIdAsync(string orderId)
        {
            OrderResponse result = null;
            try
            {
                var privateApiKey = _keyVaultCache.GetCachedSecret(nameof(BitPanda) + "ApiSecret");
                var urlPath = "/account/orders/" + orderId;

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", privateApiKey);


                var response = await httpClient.GetAsync(baseUrl + urlPath);
                if (response.IsSuccessStatusCode)
                {
                    Console.WriteLine(await response.Content.ReadAsStringAsync());

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        result = await JsonSerializer.DeserializeAsync<OrderResponse>(stream);

                    }
                }
            }
            catch
            {
            }

            return result;


        }
        public async Task<bool> BuyLimitOrderAsync(Arbitrage arbitrage)
        {
            if (!Config.ProcessTrades)
            {
                var comment = "BuyLimitOrderAsync skipped. ProcessTrades is not activated";
                _presenter.Warning(comment);
                arbitrage.BuyComment = comment;

                return false;
            }
            _presenter.ShowInfo($"Let's buy");


            CreateOrderResponse buyResponse = null;
            try
            {
                var pair = GetLongPair(arbitrage.Pair);
                if (!Pairs.Any(p => p == pair))
                {
                    var comment = "Unsupported currency pair. Process cancel...";
                    _presenter.ShowError(comment);

                    arbitrage.BuyComment = comment;

                    return false;
                }
                buyResponse = await NewLimitOrderAsync(pair, "buy", arbitrage.BuyOrginalAmount.Value, arbitrage.BuyUnitPrice.Value);

            }
            catch (System.Exception ex)
            {
                var comment = $"Buylimit failed. {ex}. Process cancel...";

                _presenter.ShowPanic(comment);
                arbitrage.BuyComment = comment;
                 return false;
            }

            if (buyResponse == null)
            {
                var comment = $"Buyresponse is empty. Process cancel...";
                _presenter.ShowError(comment);
                arbitrage.BuyComment = comment;
                 return false;
            }

            if (!string.IsNullOrWhiteSpace(buyResponse.error))
            {

                var comment = $"Buylimit failed. {buyResponse.error}. Process cancel...";
                _presenter.ShowError(comment);
                arbitrage.BuyComment = comment;
                 return false;
            }

            if (string.IsNullOrWhiteSpace(buyResponse.order_id))
            {
                var comment = $"Buylimit failed. Order not complete. Process cancel...";
                _presenter.ShowPanic(comment);
                arbitrage.BuyComment = comment;
                 return false;
            }


            _presenter.ShowInfo($"Waiting for buy confirmation");
            arbitrage.BuyOrderId = buyResponse.order_id;
            arbitrage.BuyWhenCreated= buyResponse.time;

            OrderResponse response = null;

            bool opComplete = System.Threading.SpinWait.SpinUntil(() =>
            {
                try
                {
                    response = GetOrderByOrderIdAsync(buyResponse.order_id).Result;
                    if (response?.order != null)
                    {
                        arbitrage.BuyStatus = response.order.status;

                        arbitrage.BuyOrginalAmount = response.order.amountNum;
                        arbitrage.BuyRemainingAmount  = response.order.amountNum - response.order.filled_amountNum;
                        arbitrage.BuyCummulativeFee = response.trades.Select(p => p.fee).Sum(p => p.fee_amountNum);
                        arbitrage.BuyCummulativeFeeQuote = response.trades.Sum(p => p.fee_QuoteAmountNum);
                        arbitrage.BuyCummulativeQuoteQty = response.trades.Sum(p => p.quoteAmountNum);
                    }
                }
                catch (Exception ex)
                {
                    _presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
                }
                return response?.order != null && (response.order.status == "FILLED_FULLY" || response.order.status == "FILLED" || response.order.status == "CLOSED");


            }, TimeSpan.FromMilliseconds(Config.BuyTimeoutInMs));

            var buySuccess = arbitrage.BuyStatus != null && (arbitrage.BuyStatus == "FILLED_FULLY" || arbitrage.BuyStatus == "FILLED" || arbitrage.BuyStatus == "CLOSED");
            if (!buySuccess)
            {
                _presenter.ShowInfo($"Buylimit order was sent but could not be confirmed in time. Current state is {arbitrage?.BuyStatus} Trying to cancel the order.");

                //OPENED state
                CloseOrderByOrderIdResponse buyCancelResult = null;
                try
                {
                    buyCancelResult = await CloseOrderByOrderIdAsync(arbitrage.BuyOrderId);
                }
                catch (System.Exception ex)
                {
                    _presenter.ShowPanic($"CancelOrderAsync failed. {ex}. Maybe buy was successful.");
                }

                if (buyCancelResult != null && string.IsNullOrWhiteSpace(buyCancelResult.error))
                {
                    var comment = "Order was cancelled successfully.Process cancel...";
                    _presenter.ShowInfo(comment);

                    arbitrage.BuyComment = comment;
                     return false;
                }
                else
                {
                    var comment = $"CancelOrderAsync  exited with wrong errorcode. Assuming the trade was finished." + buyCancelResult?.error;
                    _presenter.ShowPanic(comment);
                    arbitrage.BuyComment = comment;
                     return false;
                }
            }
            arbitrage.SellOrginalAmount = arbitrage.BuyOrginalAmount.Value - arbitrage.BuyRemainingAmount.Value - arbitrage.BuyCummulativeFee;

            var shouldSell = arbitrage.SellOrginalAmount > 0.00055;
            
            var mes = $"Buy state " + arbitrage.BuyStatus;

            if (shouldSell)
            {
                _presenter.ShowInfo(mes);

            }
            else
            {
                mes += " Opened BTC position on BitPanda. Please solve manualy";
                _presenter.ShowPanic(mes);
            }

            arbitrage.BuyComment = mes;

            return shouldSell;
        }
        public Task<Tuple<bool, SellResult>> SellMarketAsync(OrderCandidate orderCandidate)
        {
            throw new Exception();
        }
        public Task PrintAccountInformationAsync()
        {
            throw new System.Exception();
        }
        private string GetLongPair(string shortPair)
        {
            if (shortPair.Length == 6)
            {
                return shortPair.Insert(3, "_");
            }
            return shortPair;
        }
        private async Task<CreateOrderResponse> NewLimitOrderAsync(string currencyPair, string offerType, double amount, double unitPrice)
        {
            var privateApiKey = _keyVaultCache.GetCachedSecret(nameof(BitPanda) + "ApiSecret");

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", privateApiKey);

            var request = new CreateOrderRequest();
            request.amount = string.Format("{0:0.##############}", amount);
            request.time_in_force = "IMMEDIATE_OR_CANCELLED";
            request.type = "LIMIT";
            request.instrument_code = currencyPair;
            request.side = offerType; //buy / sell
            request.price = string.Format("{0:0.##############}", unitPrice);

            var json = JsonSerializer.Serialize<CreateOrderRequest>(request);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var result = await httpClient.PostAsync(baseUrl + "/account/orders", data);

            if (!result.IsSuccessStatusCode)
            {
                _presenter.ShowError($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");
            }


            _presenter.ShowInfo(await result.Content.ReadAsStringAsync());

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<CreateOrderResponse>(stream);
                return res;
            }

        }

        public Task<bool> SellMarketAsync(Arbitrage orderCandidate)
        {
            throw new System.Exception();
        }
    }
}
