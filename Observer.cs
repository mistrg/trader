using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Exchanges;
using Trader.Aax;
using Trader.Binance;
using Trader.Coinmate;
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
        public Observer(ObserverContext context,IEnumerable<IExchangeLogic> exchangeLogics, Presenter presenter, Estimator estimator)
        {
            _context = context;
            _estimator = estimator;
            _presenter = presenter;
            _exchangeLogics= exchangeLogics;
        }


        public async Task RunAsync()
        {
            while (true)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                //TODO Paralelism with timelimit 
                // var ro = await _aaxLogic.GetOrderBookAsync("BTCEUR");
                // var bi = await _binanceLogic.GetOrderBookAsync("BTCEUR");
                // var cm = await _coinmateLogic.GetOrderBookAsync("BTC_EUR");
                // var bp = await _bitpanda.GetOrderBookAsync();
                // var cl = await _cryptology.GetOrderBookAsync();
                // var io = await _indoex.GetOrderBookAsync();
                // var fo = await _folgory.GetOrderBookAsync();
                List<DBItem> res = new List<DBItem>();
                foreach (var logic in _exchangeLogics)
                {
                    res.AddRange(await logic.GetOrderBookAsync());
                }
                var oc = _estimator.Run(res);

                if (oc != null)
                {
                    _presenter.PrintOrderCandidate(oc);
                    await _context.CreateOrSkipOrderCandidateAsync(oc);

                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;


                Console.WriteLine($"Last cycle {ts.TotalSeconds} seconds");
            }
        }

    }
}