using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Trader;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Exchanges
{
    public class Folgory : IExchangeLogic
    {
        const string pair = "BTC_EUR";
        public class Data
        {
            public int timestamp { get; set; }
            public List<List<string>> asks { get; set; }
            public List<List<string>> bids { get; set; }
        }

        public class Root
        {
            public string message { get; set; }
            public Data data { get; set; }
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

                    var response = await httpClient.GetAsync("https://folgory.com/market/order_book?symbol=" + pair);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.data.asks)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Folgory), Pair = upair, amount = double.Parse(item[0]), askPrice = double.Parse(item[1]) });
                            foreach (var item in res.data.bids)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Folgory), Pair = upair, amount = double.Parse(item[0]), bidPrice = double.Parse(item[1]) });
                        }

                    }
                }
            }
            catch (System.Exception ex)
            {
                //Debug.Write(this); 
            }
            return result;

        }

        public double GetTradingTakerFeeRate()
        {
            return 0.0020;
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
