using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Trader.Infrastructure;
using Trader.PostgresDb;
using System.Text;
using Trader.Exchanges;
using System.Web;

namespace Trader.BitBay
{


    public class BitBayLogic : BaseExchange, IExchangeLogic
    {
        private string _publicApiKey = "b0f78148-f751-40b4-b645-a39261c60354";
        private string _privateApiKey = "e97656df-60cd-4116-b6df-369e8124eb51";

        private string baseUrl = "https://api.bitbay.net/rest/";
        public List<string> Pairs { get; }

        private static readonly HttpClient httpClient = new HttpClient();

        private readonly Presenter _presenter;


        public BitBayLogic(Presenter presenter, ObserverContext context)
                : base(context)
        {
            Pairs = new List<string>() { "BTC-EUR" };
            _presenter = presenter;
        }


        const string pair = "BTC-EUR";





        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            OrderBookTotalCount++;
            var upair = pair.Replace("-", "");


            var result = new List<DBItem>();
            try
            {
                using (HttpClient httpClient = GetHttpClient())
                {


                    var response = await httpClient.GetAsync("https://api.bitbay.net/rest/trading/orderbook/" + pair);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<OrderBookResponse>(stream);

                            foreach (var item in res.buy)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(BitBay), Pair = upair, amount = double.Parse(item.ca), askPrice = double.Parse(item.ra) });
                            foreach (var item in res.sell)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(BitBay), Pair = upair, amount = double.Parse(item.ca), bidPrice = double.Parse(item.ra) });
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

        public double GetTradingTakerFeeRate()
        {
            return 0.0043;

        }

        public async Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            try
            {

                var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var signatureInput = _publicApiKey + nonce;

                var hashHMACHex = Cryptography.HashHMAC512Hex(signatureInput, _privateApiKey);

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("API-Key", _publicApiKey);
                httpClient.DefaultRequestHeaders.Add("API-Hash", hashHMACHex.ToLower());
                httpClient.DefaultRequestHeaders.Add("operation-id", Guid.NewGuid().ToString());
                httpClient.DefaultRequestHeaders.Add("Request-Timestamp", nonce.ToString());

                var response = await httpClient.GetAsync(baseUrl + "balances/BITBAY/balance");
                if (response.IsSuccessStatusCode)
                {

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var res = await JsonSerializer.DeserializeAsync<BalanceRoot>(stream);
                        if (res.status != "Ok")
                            throw new Exception("Status not ok");

                        var btc = res.balances.SingleOrDefault(p => p.currency == currencyPair.Substring(0, 3));
                        var euro = res.balances.SingleOrDefault(p => p.currency == currencyPair.Substring(3, 3));

                        return new Tuple<double?, double?>(btc?.availableFunds, euro?.availableFunds);

                    }


                }
            }
            catch
            {
            }
            throw new Exception("Counldn't get the balance");

        }

        private string GetLongPair(string shortPair)
        {
            if (shortPair.Length == 6)
            {
                return shortPair.Insert(3, "-");
            }
            return shortPair;
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

            OfferResponse buyResponse = null;
            long? buyTime;
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
                buyTime = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                buyResponse = await NewLimitOrderAsync(pair, "buy", orderCandidate.Amount, orderCandidate.UnitAskPrice);

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

            if (buyResponse.errors != null && buyResponse.errors.Count > 0)
            {

                var comment = $"Buylimit failed. {buyResponse.errors.Concat(new string[] { "," })}. Process cancel...";
                _presenter.ShowError(comment);
                result.Comment = comment;
                return new Tuple<bool, BuyResult>(false, result);
            }

            if (string.IsNullOrWhiteSpace(buyResponse.offerId))
            {
                var comment = $"Buylimit failed. Order not complete. Process cancel...";
                _presenter.ShowPanic(comment);
                result.Comment = comment;
                return new Tuple<bool, BuyResult>(false, result);
            }

            _presenter.ShowInfo($"Waiting for buy confirmation");
            result.OrderId = buyResponse.offerId;
            result.Status = buyResponse.status;
            result.OriginalAmount = orderCandidate.Amount;
            result.RemainingAmount = orderCandidate.Amount - buyResponse.CompletedAmount;
            result.Price = orderCandidate.UnitAskPrice;
            result.Timestamp = buyTime.Value;

            try
            {
                var response = GetTransactionsHistoryAsync(offerId: result.OrderId).Result;

                result.CummulativeFee = response.CummulativeFee;

            }
            catch (Exception ex)
            {
                _presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
            }


            orderCandidate.Amount -= result.RemainingAmount.Value;




            _presenter.ShowInfo($"Buy successful");

            return new Tuple<bool, BuyResult>(true, result);

        }

        public Task<Tuple<bool, SellResult>> SellMarketAsync(OrderCandidate orderCandidate)
        {
            throw new Exception();
        }

        public async Task<ActiveOrderResponse> GetActiveOrdersAsync(string currencyPair)
        {
            using (HttpClient httpClient = GetHttpClient())
            {

                var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

                var signatureInput = _publicApiKey + nonce;

                var hashHMACHex = Cryptography.HashHMAC512Hex(signatureInput, _privateApiKey);

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("API-Key", _publicApiKey);
                httpClient.DefaultRequestHeaders.Add("API-Hash", hashHMACHex.ToLower());
                httpClient.DefaultRequestHeaders.Add("operation-id", Guid.NewGuid().ToString());
                httpClient.DefaultRequestHeaders.Add("Request-Timestamp", nonce.ToString());


                var result = await httpClient.GetAsync(baseUrl + "trading/offer/" + currencyPair);


                if (!result.IsSuccessStatusCode)
                {
                    _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");
                }

                using (var stream = await result.Content.ReadAsStreamAsync())
                {
                    var res = await JsonSerializer.DeserializeAsync<ActiveOrderResponse>(stream);
                    return res;
                }

            }

        }


        public async Task<TransactionHistoryResponse> GetTransactionsHistoryAsync(string currencyPair = "", string offerId = "")
        {

            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var signatureInput = _publicApiKey + nonce;

            var hashHMACHex = Cryptography.HashHMAC512Hex(signatureInput, _privateApiKey);

            var request = new QueryRequest();

            if (!string.IsNullOrWhiteSpace(currencyPair))
                request.markets = new List<string>() { currencyPair };

            if (!string.IsNullOrWhiteSpace(offerId))
                request.offerId = offerId;



            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("API-Key", _publicApiKey);
            httpClient.DefaultRequestHeaders.Add("API-Hash", hashHMACHex.ToLower());
            httpClient.DefaultRequestHeaders.Add("operation-id", Guid.NewGuid().ToString());
            httpClient.DefaultRequestHeaders.Add("Request-Timestamp", nonce.ToString());

            string xr = JsonSerializer.Serialize(request);

            var qs = HttpUtility.UrlEncode(xr);


            var result = await httpClient.GetAsync(baseUrl + "trading/history/transactions?query=" + qs);


            if (!result.IsSuccessStatusCode)
            {
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");
            }

            var resa = await result.Content.ReadAsStringAsync();
            Console.WriteLine(resa);

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<TransactionHistoryResponse>(stream);
                return res;
            }


        }

        public async Task<OfferResponse> NewLimitOrderAsync(string currencyPair, string offerType, double amount, double rate)
        {
            var request = new OfferRequest();
            request.amount = amount;
            request.rate = rate;
            request.offerType = offerType; //buy / sell
            request.mode = "limit";
            request.immediateOrCancel = true;

            var json = JsonSerializer.Serialize<OfferRequest>(request);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            var signatureInput = _publicApiKey + nonce + json;

            var hashHMACHex = Cryptography.HashHMAC512Hex(signatureInput, _privateApiKey);

            httpClient.DefaultRequestHeaders.Clear();
            httpClient.DefaultRequestHeaders.Add("API-Key", _publicApiKey);
            httpClient.DefaultRequestHeaders.Add("API-Hash", hashHMACHex.ToLower());
            httpClient.DefaultRequestHeaders.Add("operation-id", Guid.NewGuid().ToString());
            httpClient.DefaultRequestHeaders.Add("Request-Timestamp", nonce.ToString());


            var result = await httpClient.PostAsync(baseUrl + "trading/offer/" + currencyPair, data);


            if (!result.IsSuccessStatusCode)
            {
                _presenter.ShowPanic($"Error HTTP: {result.StatusCode} {result.ReasonPhrase}");
            }


            _presenter.ShowInfo(await result.Content.ReadAsStringAsync());

            using (var stream = await result.Content.ReadAsStreamAsync())
            {
                var res = await JsonSerializer.DeserializeAsync<OfferResponse>(stream);
                return res;
            }

        }
        public Task PrintAccountInformationAsync()
        {
            throw new System.Exception();
        }
    }
}
