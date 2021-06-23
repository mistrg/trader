using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Trader;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Trader.Exchanges
{
    public class Bitvavo : BaseExchange, IExchangeLogic
    {
        public Bitvavo(ObserverContext context)
                : base(context)
        {
        }
        const string pair = "BTC-EUR";
        public class Root
        {
            public string market { get; set; }
            public int nonce { get; set; }
            public List<List<string>> bids { get; set; }
            public List<List<string>> asks { get; set; }
        }





        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            OrderBookTotalCount++;
            var upair = pair.Replace("-", "");

            var result = new List<DBItem>();
            try
            {
                using (HttpClient httpClient = GetHttpClient())
                {


                    var response = await httpClient.GetAsync($"https://api.bitvavo.com/v2/{pair}/book");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);
                            var fee = await GetTradingTakerFeeRateAsync();

                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Bitvavo), Pair = upair, amount = double.Parse(item[1]), askPrice = double.Parse(item[0]) });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Bitvavo), Pair = upair, amount = double.Parse(item[1]), bidPrice = double.Parse(item[0]) });
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
            return 0.0025;

        }

        public Task<Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            throw new Exception();
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
