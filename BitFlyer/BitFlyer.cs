using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Trader.Exchanges;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Trader.BitFlyer
{
    public class BitFlyerLogic : BaseExchange, IExchangeLogic
    {

        public List<string> Pairs { get; }

        private string baseUrl = "https://api.bitflyer.com";

        const string pair = "BTC_EUR";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly Presenter _presenter;
        private readonly KeyVaultCache _keyVaultCache;

        public BitFlyerLogic(Presenter presenter, KeyVaultCache keyVaultCache, ObserverContext context)
                : base(context)
        {
            _keyVaultCache = keyVaultCache;
            _presenter = presenter;
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
                    var response = await httpClient.GetAsync($"{baseUrl}/v1/getboard?product_code={pair}");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<OrderBookResponse>(stream);
                            var fee = await GetTradingTakerFeeRateAsync();
                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(BitFlyer), Pair = upair, amount = item.size, askPrice = item.price });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(BitFlyer), Pair = upair, amount = item.size, bidPrice = item.price });
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
            if (WhenTradingFeeLastCheck == null || WhenTradingFeeLastCheck < DateTime.Now.AddDays(1))
            {
                try
                {
                    var publicApiKey = _keyVaultCache.GetCachedSecret("BitFlyerApiKey");
                    var privateApiKey = _keyVaultCache.GetCachedSecret("BitFlyerApiSecret");
                    var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                    var urlPath = "/v1/me/gettradingcommission?product_code=" + pair;
                    var jsonBody = "";

                    var signatureInput = $"{nonce}GET{urlPath}{jsonBody}";

                    var hashHMACHex = Cryptography.HashHMACHex(privateApiKey, signatureInput);
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("ACCESS-KEY", publicApiKey);
                    httpClient.DefaultRequestHeaders.Add("ACCESS-SIGN", hashHMACHex);
                    httpClient.DefaultRequestHeaders.Add("ACCESS-TIMESTAMP", nonce.ToString());

                    var response = await httpClient.GetAsync(baseUrl + urlPath);
                    if (response.IsSuccessStatusCode)
                    {

                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<GetComminssionResponse>(stream);
                            WhenTradingFeeLastCheck = DateTime.Now;
                            TradingFee = res.commission_rate;

                        }


                    }
                }
                catch
                {
                }


            }
            //            return 0.001;
            if (TradingFee == 0)
                throw new Exception("Invalid trading fee");

            return TradingFee;

        }
        private string GetLongPair(string shortPair)
        {
            if (shortPair.Length == 6)
            {
                return shortPair.Insert(3, "_");
            }
            return shortPair;
        }


        public async Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            try
            {
                var publicApiKey = _keyVaultCache.GetCachedSecret("BitFlyerApiKey");
                var privateApiKey = _keyVaultCache.GetCachedSecret("BitFlyerApiSecret");
                var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var urlPath = "/v1/me/getbalance";
                var jsonBody = "";

                var signatureInput = $"{nonce}GET{urlPath}{jsonBody}";

                var hashHMACHex = Cryptography.HashHMACHex(privateApiKey, signatureInput);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("ACCESS-KEY", publicApiKey);
                httpClient.DefaultRequestHeaders.Add("ACCESS-SIGN", hashHMACHex);
                httpClient.DefaultRequestHeaders.Add("ACCESS-TIMESTAMP", nonce.ToString());

                var response = await httpClient.GetAsync(baseUrl + urlPath);
                if (response.IsSuccessStatusCode)
                {

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var res = await JsonSerializer.DeserializeAsync<List<GetBalanceResponse>>(stream);


                        var btc = res.SingleOrDefault(p => p.currency_code == currencyPair.Substring(0, 3));
                        var euro = res.SingleOrDefault(p => p.currency_code == currencyPair.Substring(3, 3));

                        return new Tuple<double?, double?>(btc?.available, euro?.available);

                    }


                }
            }
            catch
            {
            }
            throw new Exception("Counldn't get the balance");
        }

        public Task<Tuple<bool, BuyResult>> BuyLimitOrderAsync(OrderCandidate orderCandidate)
        {
            throw new Exception();
        }

        public Task<Tuple<bool, SellResult>> SellMarketAsync(OrderCandidate orderCandidate)
        {
            throw new Exception();
        }
        public Task PrintAccountInformationAsync()
        {
            throw new Exception();
        }
    }
}
