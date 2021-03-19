using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Trader.PostgresDb;


namespace Trader
{


    //Kraken https://api.kraken.com/0/public/Depth?pair=BTCEUR
    //Coinbase https://api.pro.coinbase.com/products/BTC-EUR/book?level=2


    public class Observer
    {
        private readonly ObserverContext _context;

        private readonly Estimator _estimator;
        private readonly IEnumerable<IExchangeLogic> _exchangeLogics;

        private readonly Presenter _presenter;
        public Observer(ObserverContext context, IEnumerable<IExchangeLogic> exchangeLogics, Presenter presenter, Estimator estimator)
        {
            _context = context;
            _estimator = estimator;
            _presenter = presenter;
            _exchangeLogics = exchangeLogics;
        }


        public async Task RunAsync()
        {
            var lastMinute = DateTime.Now.Minute;
            while (true)
            {

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();


                var tasks = _exchangeLogics.Select(ex => ex.GetOrderBookAsync());
                var users = await Task.WhenAll(tasks);

                var res2 = users.SelectMany(p => p);


                var oc = _estimator.Run(res2);


                if (oc != null)
                {
                    await _context.CreateOrSkipOrderCandidateAsync(oc);
                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                if (lastMinute != DateTime.Now.Minute)
                {
                    lastMinute = DateTime.Now.Minute;
                    //Every minute
                    foreach (var logic in _exchangeLogics)
                        await logic.SaveTelemetryAsync();
                        
                    await _context.SaveChangesAsync();
                }
            }
        }
    }
}