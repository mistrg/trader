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
    public class B2bx : BaseExchange, IExchangeLogic
    {
        public B2bx(ObserverContext context)
                : base(context)
        {
        }
        const string pair = "btc_eur";



        public class Bid
        {
            public double amount { get; set; }
            public double price { get; set; }
        }

        public class Ask
        {
            public double amount { get; set; }
            public double price { get; set; }
        }

        public class Root
        {
            public string instrument { get; set; }
            public List<Bid> bids { get; set; }
            public List<Ask> asks { get; set; }
            public int version { get; set; }
            public double askTotalAmount { get; set; }
            public double bidTotalAmount { get; set; }
            public bool snapshot { get; set; }
        }




        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            OrderBookTotalCount++;
            var upair = pair.Replace("_", "").ToUpper();

            var result = new List<DBItem>();
            try
            {
                using (HttpClient httpClient = GetHttpClient())
                {


                    var response = await httpClient.GetAsync("https://api.b2bx.exchange:8443/trading/marketdata/instruments/" + pair + "/depth");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);
                            var fee = await GetTradingTakerFeeRateAsync();
                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(B2bx), Pair = upair, amount = item.amount, askPrice = item.price });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = fee, Exch = nameof(B2bx), Pair = upair, amount = item.amount, bidPrice = item.price });
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
            return 0.0015;

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
    }
}
