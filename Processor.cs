using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Trader.Binance;
using Trader.Coinmate;
using Trader.PostgresDb;

namespace Trader
{
    public class Processor
    {
        private readonly PostgresContext _context;
        private readonly Presenter _presenter;
        private readonly CoinmateLogic _coinmateLogic;
        private readonly BinanceLogic _binanceLogic;
        public Processor(PostgresContext context, Presenter presenter, CoinmateLogic coinmateLogic, BinanceLogic binanceLogic)
        {
            _context = context;
            _presenter = presenter;
            _coinmateLogic = coinmateLogic;
            _binanceLogic = binanceLogic;
        }

        public async Task ProcessOrderAsync(OrderCandidate orderCandidate)
        {


            // if (!((orderCandidate.BuyExchange == nameof(Trader.Coinmate) && orderCandidate.SellExchange == nameof(Trader.Binance))   ||
            // (orderCandidate.BuyExchange == nameof(Trader.Binance) && orderCandidate.SellExchange == nameof(Trader.Coinmate))))

            if (!(orderCandidate.BuyExchange == nameof(Trader.Coinmate) && orderCandidate.SellExchange == nameof(Trader.Binance)))
            {
                _presenter.ShowError($"Unsupported exchnages. Cannot buy on {orderCandidate.BuyExchange} and sell on {orderCandidate.SellExchange} Process cancel...");
                return;
            }


            var arbitrage = _context.MakeArbitrageObj(orderCandidate);
            _context.Arbitrages.Add(arbitrage);

            var buyLogic = ResolveExchangeLogic(orderCandidate.BuyExchange);
            var eurosAvailable = await buyLogic.GetAvailableAmountAsync("EUR"); //Always buying BTC for EUR

            arbitrage.BeforeBuyExchangeAvailableAmount = eurosAvailable;

            if (eurosAvailable < (orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee)
            {
                var message = $"{orderCandidate.BuyExchange} balance of {eurosAvailable } EURO too low for trade (required {(orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee} EURO). Process cancel...";
                _presenter.ShowError(message);
                await _presenter.SendMessageAsync($"Not enough EURO balance on {orderCandidate.BuyExchange}", message);
                return;
            }


            var sellLogic = ResolveExchangeLogic(orderCandidate.SellExchange);


            var btcFunds = await sellLogic.GetAvailableAmountAsync("BTC"); //Always selling BTC for EUR

            arbitrage.BeforeSellExchangeAvailableAmount = btcFunds;
            if (btcFunds < orderCandidate.Amount)
            {
                var message = $"{orderCandidate.SellExchange} balance of {btcFunds } BTC too low for trade (required {orderCandidate.Amount} BTC). Process cancel...";
                _presenter.ShowError(message);
                await _presenter.SendMessageAsync($"Not enough BTC balance on {orderCandidate.SellExchange}", message);
                return;
            }



            _presenter.ShowInfo("Starting arbitrage...");

            var buyResult = await buyLogic.BuyLimitOrderAsync(orderCandidate);

            if (!buyResult.Item1)
            {

                await _presenter.SendMessageAsync($"Arbitrage failed", "BuyLimitOrderAsync failed. Status: {buyResult.Item2?.status} Check OrderCandidate " + orderCandidate.Id, arbitrage);
                await _context.SaveChangesAsync();
                return;

            }
            //_context.EnrichBuy(arbitrage, buyResult.Item2);


            var sellResult = await sellLogic.SellMarketAsync(orderCandidate);

            if (!sellResult.Item1)
            {
                await _presenter.SendMessageAsync("Arbitrage failed", "SellMarketAsync failed. Check OrderCandidate " + orderCandidate.Id, arbitrage);
                await _context.SaveChangesAsync();
                return;
            }

           // _context.EnrichSell(arbitrage, sellResult.Item2);


          
            var successMessage = $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Arbitrage success. OCID {orderCandidate.Id} estNetProfit {orderCandidate.EstProfitNet} RealProfitNet {Math.Round(arbitrage.RealProfitNet ?? 0, 2)}";

            _presenter.ShowSuccess(successMessage);

            arbitrage.Comment = successMessage;
            arbitrage.IsSuccess = true;
            arbitrage.AfterBuyExchangeAvailableAmount = await buyLogic.GetAvailableAmountAsync("EUR"); //Always buying BTC for EUR
            arbitrage.AfterSellExchangeAvailableAmount = await sellLogic.GetAvailableAmountAsync("BTC"); //Always selling BTC for EUR
            await _context.SaveChangesAsync();
            _presenter.ShowInfo("Ending aritrage...");

            await _presenter.SendMessageAsync("Arbitrage succeded", successMessage, arbitrage);

        }


        

        private IExchangeLogic ResolveExchangeLogic(string exchange)
        {
            switch (exchange)
            {
                case nameof(Coinmate):
                    return _coinmateLogic;
                case nameof(Binance):
                    return _binanceLogic;
                default:
                    throw new Exception("Invalid exchnage");
            }
        }

    }

}