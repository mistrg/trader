using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Trader.Exchanges;
using Trader.Infrastructure;
using Trader.PostgresDb;
using Trader;
using System.Linq;

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


                    var response = await httpClient.GetAsync("https://api.exchange.bitpanda.com/public/v1/order-book/" + pair + "?depth=100");

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

                    var urlP    ath = "/account/fees";
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("X-API-KEY", privateApiKey);

                    var response = await httpClient.GetAsync(baseUrl + urlPath);
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine(response.Content.ReadAsStringAsync());
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<TradingFeeResponse>(stream);
                            WhenTradingFeeLastCheck = DateTime.Now;
                            //TradingFee = res.commission_rate;

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

        public async Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            double btcBalance = 0;
            double euroBalance = 0;

            try
            {
                var privateApiKey = _keyVaultCache.GetCachedSecret(nameof(BitPanda) + "ApiSecret");
                var urlPath = "/wallets";

                httpClient.DefaultRequestHeaders.Clear();
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", privateApiKey);

                var response = await httpClient.GetAsync(baseUrl + urlPath);
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var res = await JsonSerializer.DeserializeAsync<WalletResponse>(stream);
                        var btcBalances = res.data.Where(x => x.type == "wallet" && x.attributes.is_default && !x.attributes.deleted && x.attributes.cryptocoin_symbol == currencyPair.Substring(0, 3)).Select(p => p.attributes.balance);
                        foreach (var item in btcBalances)
                            btcBalance += double.Parse(item);
                    }
                }


                urlPath = "/fiatwallets";

                response = await httpClient.GetAsync(baseUrl + urlPath);
                if (response.IsSuccessStatusCode)
                {
                    using (var stream = await response.Content.ReadAsStreamAsync())
                    {
                        var res = await JsonSerializer.DeserializeAsync<FiatWalletResponse>(stream);
                        var euroBalances = res.data.Where(x => x.type == "fiat_wallet" && x.attributes.fiat_symbol == currencyPair.Substring(3, 3)).Select(p => p.attributes.balance);
                        foreach (var item in euroBalances)
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
            throw new System.Exception();
        }
    }
}
