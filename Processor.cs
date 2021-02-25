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


            if (!((orderCandidate.BuyExchange == nameof(Trader.Coinmate) && orderCandidate.SellExchange == nameof(Trader.Binance)) ||
            (orderCandidate.BuyExchange == nameof(Trader.Binance) && orderCandidate.SellExchange == nameof(Trader.Coinmate))))

            //if (!(orderCandidate.BuyExchange == nameof(Trader.Coinmate) && orderCandidate.SellExchange == nameof(Trader.Binance)))
            {
                _presenter.ShowError($"Unsupported exchnages. Cannot buy on {orderCandidate.BuyExchange} and sell on {orderCandidate.SellExchange} Process cancel...");
                return;
            }


            var arbitrage = _context.MakeArbitrageObj(orderCandidate);
            _context.Arbitrages.Add(arbitrage);

            var buyLogic = ResolveExchangeLogic(orderCandidate.BuyExchange);
            var buyAvailable = await buyLogic.GetAvailableAmountAsync("BTCEUR"); //Always buying BTC for EUR

            arbitrage.BeforeBuyExchangeAvailableBaseAmount = buyAvailable.Item1;
            arbitrage.BeforeBuyExchangeAvailableQuoteAmount = buyAvailable.Item2;

            if (arbitrage.BeforeBuyExchangeAvailableQuoteAmount < (orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee)
            {
                var message = $"{orderCandidate.BuyExchange} balance of {arbitrage.BeforeBuyExchangeAvailableQuoteAmount } EURO too low for trade (required {(orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee} EURO). Process cancel...";
                _presenter.ShowError(message);
                await _presenter.SendMessageAsync($"Not enough EURO balance on {orderCandidate.BuyExchange}", message);
                return;
            }


            var sellLogic = ResolveExchangeLogic(orderCandidate.SellExchange);


            var sellAvailable = await sellLogic.GetAvailableAmountAsync("BTCEUR"); //Always selling BTC for EUR

            arbitrage.BeforeSellExchangeAvailableBaseAmount = sellAvailable.Item1;
            arbitrage.BeforeSellExchangeAvailableQuoteAmount = sellAvailable.Item2;
            if (arbitrage.BeforeSellExchangeAvailableBaseAmount < orderCandidate.Amount)
            {
                var message = $"{orderCandidate.SellExchange} balance of {arbitrage.BeforeSellExchangeAvailableBaseAmount } BTC too low for trade (required {orderCandidate.Amount} BTC). Process cancel...";
                _presenter.ShowError(message);
                await _presenter.SendMessageAsync($"Not enough BTC balance on {orderCandidate.SellExchange}", message);
                return;
            }



            _presenter.ShowInfo("Starting arbitrage...");

            var buyResult = await buyLogic.BuyLimitOrderAsync(orderCandidate);
            _presenter.ShowBuyResult(buyResult);

            _context.EnrichBuy(arbitrage, buyResult.Item2);


            if (!buyResult.Item1)
            {
                await FinalizeArbitrage(buyLogic, sellLogic, arbitrage);

                await _presenter.SendMessageAsync($"Arbitrage failed", $"BuyLimitOrderAsync failed. Status: {buyResult.Item2?.Status} Check OrderCandidate " + orderCandidate.Id, arbitrage);

                return;
            }


            var sellResult = await sellLogic.SellMarketAsync(orderCandidate);
            _presenter.ShowSellResult(sellResult);

            _context.EnrichSell(arbitrage, sellResult.Item2);

            if (!sellResult.Item1)
            {
                await FinalizeArbitrage(buyLogic, sellLogic, arbitrage);

                await _presenter.SendMessageAsync("Arbitrage failed", "SellMarketAsync failed. Check OrderCandidate " + orderCandidate.Id, arbitrage);
                return;
            }



            var successMessage = $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Arbitrage success. OCID {orderCandidate.Id} estNetProfit {orderCandidate.EstProfitNet} ({orderCandidate.EstProfitNetRate}%) RealProfitNet {Math.Round(arbitrage.RealProfitNet ?? 0, 2)} ({arbitrage.RealProfitNetRate}%)";


            _presenter.ShowSuccess(successMessage);


            arbitrage.IsSuccess = true;
            await FinalizeArbitrage(buyLogic, sellLogic, arbitrage);

            _presenter.ShowInfo("Ending aritrage...");
            await _presenter.SendMessageAsync("Arbitrage succeded", successMessage, arbitrage);

        }


        private async Task FinalizeArbitrage(IExchangeLogic buyLogic, IExchangeLogic sellLogic, Arbitrage arbitrage)
        {
            var buyAvailable = await buyLogic.GetAvailableAmountAsync("BTCEUR"); //Always buying BTC for EUR
            arbitrage.AfterBuyExchangeAvailableBaseAmount = buyAvailable.Item1;
            arbitrage.AfterBuyExchangeAvailableQuoteAmount = buyAvailable.Item2;

            var sellAvailable = await sellLogic.GetAvailableAmountAsync("BTCEUR"); //Always selling BTC for EUR
            arbitrage.AfterSellExchangeAvailableBaseAmount = sellAvailable.Item1;
            arbitrage.AfterSellExchangeAvailableQuoteAmount = sellAvailable.Item2;


            arbitrage.WalletBaseAmountSurplus = (arbitrage.AfterSellExchangeAvailableBaseAmount - arbitrage.BeforeSellExchangeAvailableBaseAmount) + (arbitrage.AfterBuyExchangeAvailableBaseAmount - arbitrage.BeforeBuyExchangeAvailableBaseAmount);
            arbitrage.WalletQuoteAmountSurplus = (arbitrage.AfterSellExchangeAvailableQuoteAmount - arbitrage.BeforeSellExchangeAvailableQuoteAmount) + (arbitrage.AfterBuyExchangeAvailableQuoteAmount - arbitrage.BeforeBuyExchangeAvailableQuoteAmount);

            //Compute Wallet difference
            await _context.SaveChangesAsync();

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