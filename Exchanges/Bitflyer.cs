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
    public class Bitflyer : IExchangeLogic
    {
        const string pair = "BTC_EUR";
      
    public class Bid
    {
        public double price { get; set; }
        public double size { get; set; }
    }

    public class Ask
    {
        public double price { get; set; }
        public double size { get; set; }
    }

    public class Root
    {
        public double mid_price { get; set; }
        public List<Bid> bids { get; set; }
        public List<Ask> asks { get; set; }
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

                    var response = await httpClient.GetAsync($"https://api.bitflyer.com/v1/getboard?product_code={pair}");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Bitflyer), Pair = upair, amount = item.size, askPrice = item.price });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Bitflyer), Pair = upair, amount = item.size, bidPrice =item.price });
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
            return 0.001;

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
