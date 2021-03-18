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
    public class Liquid : IExchangeLogic
    {
        const string pair = "BTCEUR";
      
    public class Root
    {
        public List<List<string>> buy_price_levels { get; set; }
        public List<List<string>> sell_price_levels { get; set; }
        public string timestamp { get; set; }
    }


        public async Task<List<DBItem>> GetOrderBookAsync()
        {
            var upair = pair.Replace("-", "");

            var result = new List<DBItem>();
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.Timeout = TimeSpan.FromMilliseconds(1000);

                    var response = await httpClient.GetAsync("https://api.liquid.com/products/3/price_levels");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.buy_price_levels)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Liquid), Pair = upair, amount = double.Parse(item[1]), askPrice = double.Parse(item[0]) });
                            foreach (var item in res.sell_price_levels)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Liquid), Pair = upair, amount = double.Parse(item[1]), bidPrice = double.Parse(item[0]) });
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
