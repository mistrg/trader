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
    public class Luno : BaseExchange, IExchangeLogic
    {
        public Luno(ObserverContext context)
                : base(context)
        {
        }
        const string pair = "XBTEUR";

        public class Ask
        {
            public string price { get; set; }
            public string volume { get; set; }
        }

        public class Bid
        {
            public string price { get; set; }
            public string volume { get; set; }
        }

        public class Root
        {
            public long timestamp { get; set; }
            public List<Ask> asks { get; set; }
            public List<Bid> bids { get; set; }
        }






        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            OrderBookTotalCount++;
            var upair = "BTCEUR";

            var result = new List<DBItem>();
            try
            {
                using (HttpClient httpClient = GetHttpClient())
                {


                    var response = await httpClient.GetAsync($"https://api.luno.com/api/1/orderbook_top?pair={pair}");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);
                            var fee = await GetTradingTakerFeeRateAsync();

                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Luno), Pair = upair, amount = double.Parse(item.volume), askPrice = double.Parse(item.price) });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(Luno), Pair = upair, amount = double.Parse(item.volume), bidPrice = double.Parse(item.price) });
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
  
             return await Task.FromResult(0.001);


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

        Task IExchangeLogic.SaveTelemetryAsync()
        {
            throw new System.Exception();
        }

        Task<double> IExchangeLogic.GetTradingTakerFeeRateAsync()
        {
            throw new System.Exception();
        }

        Task<Tuple<double?, double?>> IExchangeLogic.GetAvailableAmountAsync(string currencyPair)
        {
            throw new System.Exception();
        }

        Task<bool> IExchangeLogic.BuyLimitOrderAsync(Arbitrage arbitrage)
        {
            throw new System.Exception();
        }

        Task<bool> IExchangeLogic.SellMarketAsync(Arbitrage orderCandidate)
        {
            throw new System.Exception();
        }

        Task<List<DBItem>> IExchangeLogic.GetOrderBookAsync()
        {
            throw new System.Exception();
        }

        Task IExchangeLogic.PrintAccountInformationAsync()
        {
            throw new System.Exception();
        }
    }
}
