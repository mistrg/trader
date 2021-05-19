using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        private string _publicApiKey = "CwQ7vZcFcPbAsi1dN3au8E";
        private string _privateApiKey = "q5uPgau2w2Q4o8Ks+r+CVeXxiv72DJ4W1HGzxZKrEkU=";


        private string baseUrl = "https://api.bitflyer.com";

        const string pair = "BTC_EUR";
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly Presenter _presenter;

        public BitFlyerLogic(Presenter presenter, ObserverContext context)
                : base(context)
        {
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

                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(BitFlyer), Pair = upair, amount = item.size, askPrice = item.price });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(BitFlyer), Pair = upair, amount = item.size, bidPrice = item.price });
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
            return 0.001;

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

                var nonce = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                var urlPath = "/v1/me/getbalance";
                var jsonBody = "";
                
                var signatureInput = $"{nonce}GET{urlPath}{jsonBody}";

                var hashHMACHex = Cryptography.HashHMACHex(_privateApiKey,signatureInput);
                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("ACCESS-KEY", _publicApiKey);
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
