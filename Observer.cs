using System;
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
    // 70-122

    //https://coinmarketcap.com/de/rankings/exchanges/


    public class BitBay
    {

        private string Url = "https://api.bitbay.net/rest/trading/orderbook/BTC-EUR";
        //CA and RA
        //         {
        //   "status": "Ok",
        //   "sell": [
        //     {
        //       "ra": "25285.31",
        //       "ca": "0.02839638",
        //       "sa": "0.02839638",
        //       "pa": "0.02839638",
        //       "co": 1
        //     }
        //   ],
        //   "buy": [
        //     {
        //       "ra": "25280",
        //       "ca": "0.82618498",
        //       "sa": "3.59999",
        //       "pa": "0.82618498",
        //       "co": 1
        //     }
        //   ],
        //   "timestamp": "1529512856512",
        //   "seqNo": "139098"
        // }
    }



    public class Observer
    {
        private readonly ObserverContext _context;

        private readonly CoinmateLogic _coinmateLogic;
        private readonly BinanceLogic _binanceLogic;
        private readonly AaxLogic _aaxLogic;
        private readonly Estimator _estimator;
        private readonly Bitpanda _bitpanda;
        private readonly Cryptology _cryptology;
        private readonly Folgory _folgory;
        private readonly Indoex _indoex;
        
        private readonly Presenter _presenter;
        public Observer(ObserverContext context,Folgory folgory, Indoex indoex, Cryptology cryptology, Bitpanda bitpanda, Presenter presenter, Estimator estimator, CoinmateLogic coinmateLogic, BinanceLogic binanceLogic, AaxLogic aaxLogic)
        {
            _folgory = folgory;
            _indoex = indoex;
            _context = context;
            _coinmateLogic = coinmateLogic;
            _binanceLogic = binanceLogic;
            _aaxLogic = aaxLogic;
            _estimator = estimator;
            _bitpanda = bitpanda;
            _presenter = presenter;
            _cryptology = cryptology;
        }


        public async Task RunAsync()
        {
            while (true)
            {
                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                //TODO Paralelism with timelimit 
                // var ro = await _aaxLogic.GetOrderBookAsync("BTCEUR");
                var bi = await _binanceLogic.GetOrderBookAsync("BTCEUR");
                var cm = await _coinmateLogic.GetOrderBookAsync("BTC_EUR");
                var bp = await _bitpanda.GetOrderBookAsync();
                var cl = await _cryptology.GetOrderBookAsync();
                var io = await _indoex.GetOrderBookAsync();
                var fo = await _folgory.GetOrderBookAsync();
                var oc = _estimator.Run(cm.Union(bi).Union(bp).Union(cl).Union(io).Union(fo));

                if (oc != null)
                {
                    //_presenter.PrintOrderCandidate(oc);
                    await _context.CreateOrSkipOrderCandidateAsync(oc);

                }

                stopWatch.Stop();
                TimeSpan ts = stopWatch.Elapsed;

                
                //Console.WriteLine($"Last cycle {ts.TotalSeconds} seconds");
            }
        }

    }
}