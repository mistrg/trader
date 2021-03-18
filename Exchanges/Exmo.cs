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
    public class Exmo : IExchangeLogic
    {
        const string pair = "BTC_EUR";
        
        public class BTCEUR
        {
            public string ask_quantity { get; set; }
            public string ask_amount { get; set; }
            public string ask_top { get; set; }
            public string bid_quantity { get; set; }
            public string bid_amount { get; set; }
            public string bid_top { get; set; }
            public List<List<string>> ask { get; set; }
            public List<List<string>> bid { get; set; }
        }

        public class Root
        {
            public BTCEUR BTC_EUR { get; set; }
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

                    var response = await httpClient.GetAsync($"https://api.exmo.com/v1.1/order_book?pair={pair}");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.BTC_EUR.ask)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Exmo), Pair = upair, amount = double.Parse(item[1]), askPrice = double.Parse(item[0]) });
                            foreach (var item in res.BTC_EUR.bid)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Exmo), Pair = upair, amount = double.Parse(item[1]), bidPrice = double.Parse(item[0]) });
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
            return 0.003;

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
