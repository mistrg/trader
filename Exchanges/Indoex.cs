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
    public class Indoex : BaseExchange, IExchangeLogic
    {
        public Indoex(ObserverContext context)
                : base(context)
        {
        }
        const string pair = "BTC_EUR";


        public class Ask
        {
            public string price { get; set; }
            public string quantity { get; set; }
        }

        public class Bid
        {
            public string price { get; set; }
            public string quantity { get; set; }
        }

        public class Root
        {
            public int status { get; set; }
            public string message { get; set; }
            public List<Ask> asks { get; set; }
            public List<Bid> bids { get; set; }
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


                    var response = await httpClient.GetAsync("https://api.indoex.io/depth/" + pair);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);
                            var fee = await GetTradingTakerFeeRateAsync();

                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Indoex), Pair = upair, amount = double.Parse(item.quantity), askPrice = double.Parse(item.price) });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Indoex), Pair = upair, amount = double.Parse(item.quantity), bidPrice = double.Parse(item.price) });
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
            
             return await Task.FromResult(0.0015);


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
