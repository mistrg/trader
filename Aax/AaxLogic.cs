using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Trader.Infrastructure;
using Trader.PostgresDb;

namespace Trader.Aax
{
    public class AaxLogic : IExchangeLogic
    {


        static string baseUri = "https://api.aax.com/v2/";
        private static readonly HttpClient httpClient = new HttpClient();

        public async Task<List<DBItem>> GetOrderBookAsync(string pair)
        {
            var result = new List<DBItem>();
            try
            {
                var res = await httpClient.GetFromJsonAsync<OrderBookResponse>(baseUri + "market/orderbook?level=20&symbol=" + pair);

                if (res == null)
                    return result;


                foreach (var x in res.asks)
                {
                    var amount = double.Parse(x[1]);
                    var price = double.Parse(x[0]);
                    result.Add(new DBItem() { Exch = nameof(Aax), Pair = pair, amount = amount, askPrice = price });
                }


                foreach (var x in res.bids)
                {
                    var amount = double.Parse(x[1]);
                    var price = double.Parse(x[0]);

                    result.Add(new DBItem() { Exch = nameof(Aax), Pair = pair, amount = amount, bidPrice = price });

                }
            }
            catch
            {
            }
            return result;
        }

        public double GetTradingTakerFeeRate()
        {
             return 0.001;
        }

        public Task<System.Tuple<bool, BuyResult>> BuyLimitOrderAsync(OrderCandidate orderCandidate)
        {
            throw new System.Exception();
        }

        public Task<System.Tuple<bool, SellResult>> SellMarketAsync(OrderCandidate orderCandidate)
        {
            throw new System.Exception();
        }

        public Task<System.Tuple<double?, double?>> GetAvailableAmountAsync(string currencyPair)
        {
            throw new System.Exception();
        }
    }

}