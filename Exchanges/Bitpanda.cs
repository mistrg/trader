using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Trader;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Exchanges
{
    public class Bitpanda : IExchangeLogic
    {
        const string pair = "BTC_EUR";
        
        public class Bid
        {
            public string price { get; set; }
            public string amount { get; set; }
            public string order_id { get; set; }
        }

        public class Ask
        {
            public string price { get; set; }
            public string amount { get; set; }
            public string order_id { get; set; }
        }

        public class Root
        {
            public string instrument_code { get; set; }
            public DateTime time { get; set; }
            public List<Bid> bids { get; set; }
            public List<Ask> asks { get; set; }
            public int sequence { get; set; }
        }


        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            var upair = pair.Replace("_", "");

            var result = new List<DBItem>();
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(1000);

                    var response = await httpClient.GetAsync("https://api.exchange.bitpanda.com/public/v1/order-book/" + pair+"?depth=100");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Bitpanda), Pair = upair, amount = double.Parse(item.amount), askPrice = double.Parse(item.price) });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Bitpanda), Pair = upair, amount = double.Parse(item.amount), bidPrice = double.Parse(item.price) });
                        }

                    }
                }
            }
            catch (System.Exception)
            {
            }
            return result;

        }

        public double GetTradingTakerFeeRate()
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
    }
}
