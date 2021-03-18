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
    public class Gemini : IExchangeLogic
    {
        const string pair = "BTCEUR";
     
    public class Bid
    {
        public string price { get; set; }
        public string amount { get; set; }
        public string timestamp { get; set; }
    }

    public class Ask
    {
        public string price { get; set; }
        public string amount { get; set; }
        public string timestamp { get; set; }
    }

    public class Root
    {
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

                    var response = await httpClient.GetAsync("https://api.gemini.com/v1/book/" + pair);

                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            var res = await JsonSerializer.DeserializeAsync<Root>(stream);

                            foreach (var item in res.asks)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Gemini), Pair = upair, amount = double.Parse(item.amount), askPrice = double.Parse(item.price) });
                            foreach (var item in res.bids)
                                result.Add(new DBItem() { TakerFeeRate = GetTradingTakerFeeRate(), Exch = nameof(Gemini), Pair = upair, amount = double.Parse(item.amount), bidPrice = double.Parse(item.price) });
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
