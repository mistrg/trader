using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Trader.PostgresDb;

namespace Trader.Exchanges
{
    public class BaseExchange
    {
        private readonly ObserverContext _context;

        public long OrderBookTotalCount { get; set; }
        public long OrderBookSuccessCount { get; set; }
        public long OrderBookFailCount { get; set; }

        public BaseExchange(ObserverContext context)
        {
            _context = context;

        }

        public HttpClient GetHttpClient()
        {

            return new HttpClient() { Timeout = TimeSpan.FromMilliseconds(1000) };

        }


        public async Task SaveTelemetryAsync()
        {
            //Add or update 
            var db = _context.ExchangeRuntimeInfo.SingleOrDefault(p => p.BotRunId == Config.RunId && this.GetType().Name == p.ExchangeName);
            if (db != null)
            {
                db.OrderBookTotalCount = OrderBookTotalCount;
                db.OrderBookSuccessCount = OrderBookSuccessCount;
                db.OrderBookFailCount = OrderBookFailCount;
            }
            else
            {
                var newobj = new ExchangeRuntimeInfo()
                {
                    BotRunId = Config.RunId,
                    ExchangeName = this.GetType().Name,
                    OrderBookTotalCount = OrderBookTotalCount,
                    OrderBookSuccessCount = OrderBookSuccessCount,
                    OrderBookFailCount = OrderBookFailCount
                };
                await _context.ExchangeRuntimeInfo.AddAsync(newobj);
            }
        }
    }
}