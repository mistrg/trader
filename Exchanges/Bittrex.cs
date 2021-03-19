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
    public class Bitrex : BaseExchange, IExchangeLogic
    {
        public Bitrex(ObserverContext context)
                : base(context)
        {
        }
        const string pair = "BTC-EUR";


        public class Bid
        {
            public string quantity { get; set; }
            public string rate { get; set; }
        }

        public class Ask
        {
            public string quantity { get; set; }
            public string rate { get; set; }
        }

        public class Root
        {
            public List<Bid> bid { get; set; }
            public List<Ask> ask { get; set; }
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


                    var response = await httpClient.GetAsync($"https://api.bittrex.com/v3/markets/{pair}/orderbook");

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.ask)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Bitrex), Pair = upair, amount = double.Parse(item.quantity), askPrice = double.Parse(item.rate) });
                            foreach (var item in res.bid)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Bitrex), Pair = upair, amount = double.Parse(item.quantity), bidPrice = double.Parse(item.rate) });
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

        public double GetTradingTakerFeeRate()
        {
            return 0.0035;

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
