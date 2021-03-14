using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Trader.Aax;
using Trader.Binance;
using Trader.Coinmate;
using Trader.PostgresDb;

namespace Trader
{
    public class Observer
    {
        private readonly ObserverContext _context;

        private readonly CoinmateLogic _coinmateLogic;
        private readonly BinanceLogic _binanceLogic;
        private readonly AaxLogic _aaxLogic;
        private readonly Estimator _estimator;
        public Observer(ObserverContext context, Estimator estimator,  CoinmateLogic coinmateLogic, BinanceLogic binanceLogic, AaxLogic aaxLogic)
        {
            _context = context;
            _coinmateLogic = coinmateLogic;
            _binanceLogic = binanceLogic;
            _aaxLogic = aaxLogic;
            _estimator  = estimator;
        }

        public async Task RunAsync()
        {
            while (true)
            {
                //TODO Paralelism with timelimit 
               // var ro = await _aaxLogic.GetOrderBookAsync("BTCEUR");
                var bi = await _binanceLogic.GetOrderBookAsync("BTCEUR");
                var cm = await _coinmateLogic.GetOrderBookAsync("BTC_EUR");
                
                var oc  = _estimator.Run(cm.Union(bi));

                if (oc!=null)
                {
                    await _context.CreateOrSkipOrderCandidateAsync(oc);
                }


            }
        }

    }
}