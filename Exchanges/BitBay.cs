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
    public class BitBay : IExchangeLogic
    {
        const string pair = "BTC-EUR";
        public class Sell
        {
            public string ra { get; set; }
            public string ca { get; set; }
            public string sa { get; set; }
            public string pa { get; set; }
            public int co { get; set; }
        }

        public class Buy
        {
            public string ra { get; set; }
            public string ca { get; set; }
            public string sa { get; set; }
            public string pa { get; set; }
            public int co { get; set; }
        }

        public class Root
        {
            public string status { get; set; }
            public List<Sell> sell { get; set; }
            public List<Buy> buy { get; set; }
            public string timestamp { get; set; }
            public string seqNo { get; set; }
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

                    var response = await httpClient.GetAsync("https://api.bitbay.net/rest/trading/orderbook/" + pair);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.buy)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(BitBay), Pair = upair, amount = double.Parse(item.ca), askPrice = double.Parse(item.ra) });
                            foreach (var item in res.sell)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(BitBay), Pair = upair, amount = double.Parse(item.ca), bidPrice = double.Parse(item.ra) });
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
            return 0.0043;

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
