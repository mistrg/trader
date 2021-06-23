using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trader.PostgresDb;
using System.Linq;

namespace Trader
{
    public class Processor
    {
        private readonly PostgresContext _context;
        private readonly Presenter _presenter;

        private bool LowCreditWarningSent = false;

        private readonly IEnumerable<IExchangeLogic> _exchangeLogics;


        public Processor(PostgresContext context,IEnumerable<IExchangeLogic> exchangeLogics, Presenter presenter)
        {
            _context = context;
            _presenter = presenter;
            _exchangeLogics = exchangeLogics;
        }

        public async Task ProcessOrderAsync(OrderCandidate orderCandidate)
        {


            if (!((orderCandidate.BuyExchange == nameof(Trader.BitPanda) && orderCandidate.SellExchange == nameof(Trader.BitBay))))
            {
                _presenter.ShowError($"Unsupported exchnages. Cannot buy on {orderCandidate.BuyExchange} and sell on {orderCandidate.SellExchange} Process cancel...");
                return;
            }

            var buyLogic = _exchangeLogics.SingleOrDefault(p=>p.GetType().Name == orderCandidate.BuyExchange+"Logic");
            var buyAvailable = await buyLogic.GetAvailableAmountAsync(orderCandidate.Pair); //Always buying BTC for EUR

            var beforeBuyExchangeAvailableBaseAmount = buyAvailable.Item1;
            var beforeBuyExchangeAvailableQuoteAmount = buyAvailable.Item2;

            if (beforeBuyExchangeAvailableQuoteAmount == null || beforeBuyExchangeAvailableQuoteAmount < (orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee)
            {
                var message = $"{orderCandidate.BuyExchange} balance of {beforeBuyExchangeAvailableQuoteAmount } EURO too low for trade (required {(orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee} EURO). Process cancel...";
                _presenter.ShowError(message);

                if (!LowCreditWarningSent)
                {
                    LowCreditWarningSent = true;
                    await _presenter.SendMessageAsync($"Not enough EURO balance on {orderCandidate.BuyExchange}", message);
                }
                return;
            }


             var sellLogic = _exchangeLogics.SingleOrDefault(p=>p.GetType().Name == orderCandidate.SellExchange);

            var sellAvailable = await sellLogic.GetAvailableAmountAsync(orderCandidate.Pair); //Always selling BTC for EUR

            var beforeSellExchangeAvailableBaseAmount = sellAvailable.Item1;
            var beforeSellExchangeAvailableQuoteAmount = sellAvailable.Item2;
            if (beforeSellExchangeAvailableBaseAmount == null || beforeSellExchangeAvailableBaseAmount < orderCandidate.Amount)
            {
                var message = $"{orderCandidate.SellExchange} balance of {beforeSellExchangeAvailableBaseAmount } BTC too low for trade (required {orderCandidate.Amount} BTC). Process cancel...";
                _presenter.ShowError(message);
                if (!LowCreditWarningSent)
                {
                    LowCreditWarningSent = true;
                    await _presenter.SendMessageAsync($"Not enough BTC balance on {orderCandidate.SellExchange}", message);
                }
                return;
            }

            _presenter.ShowInfo("Starting arbitrage...");

            var arbitrage = _context.MakeArbitrageObj(orderCandidate);

            arbitrage.BeforeBuyExchangeAvailableBaseAmount = beforeBuyExchangeAvailableBaseAmount;
            arbitrage.BeforeBuyExchangeAvailableQuoteAmount = beforeBuyExchangeAvailableQuoteAmount;

            arbitrage.BeforeSellExchangeAvailableBaseAmount = beforeSellExchangeAvailableBaseAmount;
            arbitrage.BeforeSellExchangeAvailableQuoteAmount = beforeSellExchangeAvailableQuoteAmount;

            _context.Arbitrages.Add(arbitrage);

            var buyResult = await buyLogic.BuyLimitOrderAsync(arbitrage);

            if (!buyResult)
            {
                await FinalizeArbitrage(buyLogic, sellLogic, arbitrage);

                await _presenter.SendMessageAsync($"Arbitrage failed", $"BuyLimitOrderAsync failed. Status: {arbitrage.BuyStatus} Check OrderCandidate " + orderCandidate.Id, arbitrage);

                return;
            }


            var sellResult = await sellLogic.SellMarketAsync(arbitrage);


            if (!sellResult)
            {
                await FinalizeArbitrage(buyLogic, sellLogic, arbitrage);

                await _presenter.SendMessageAsync("Arbitrage failed", "SellMarketAsync failed. Check OrderCandidate " + orderCandidate.Id, arbitrage);
                return;
            }



            var successMessage = $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Arbitrage success. OCID {orderCandidate.Id} estNetProfit {orderCandidate.EstProfitNet} ({orderCandidate.EstProfitNetRate}%) RealProfitNet {Math.Round(arbitrage.RealProfitNet ?? 0, 2)} ({arbitrage.RealProfitNetRate}%)";


            _presenter.ShowSuccess(successMessage);


            await FinalizeArbitrage(buyLogic, sellLogic, arbitrage);

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



            arbitrage.IsSuccess = arbitrage.EstProfitNetRate <= arbitrage.RealProfitNetRate &&
                                    arbitrage.RealProfitNet > 0 && 
                                    arbitrage.WalletBaseAmountSurplus >= 0 && 
                                    arbitrage.WalletQuoteAmountSurplus >= 0;


            //Compute Wallet difference
            await _context.SaveChangesAsync();

        }







      

    }

}