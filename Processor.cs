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
        private int buyTimeoutInMs = 2500;

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

            var buyLogic  = ResolveExchangeLogic(orderCandidate.BuyExchange);
            var eurosAvailable = await buyLogic.GetAvailableAmountAsync("EUR"); //Always buying BTC for EUR

            arbitrage.BeforeBuyExchangeAvailableAmount = eurosAvailable;

            if (eurosAvailable < (orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee)
            {
                var message = $"{orderCandidate.BuyExchange} balance of {eurosAvailable } EURO too low for trade (required {(orderCandidate.Amount * orderCandidate.UnitAskPrice) + orderCandidate.EstBuyFee} EURO). Process cancel...";
                _presenter.ShowError(message);
                await _presenter.SendMessageAsync($"Not enough EURO balance on {orderCandidate.BuyExchange}", message);
                return;
            }
          

            var sellLogic  = ResolveExchangeLogic(orderCandidate.SellExchange);


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

            var buyResult = await BuyLimitOrderAsync(orderCandidate, arbitrage);

            if (!buyResult.Item1)
            {
                if (buyResult.Item2?.status?.Contains("Access denied") == true)
                    return;

                await _presenter.SendMessageAsync($"Arbitrage failed", "BuyLimitOrderAsync failed. Status: {buyResult.Item2?.status} Check OrderCandidate " + orderCandidate.Id, arbitrage);
                await _context.SaveChangesAsync();
                return;

            }

            var sellResult = await SellMarketAsync(orderCandidate, arbitrage);

            if (!sellResult.Item1)
            {
                await _presenter.SendMessageAsync("Arbitrage failed", "SellMarketAsync failed. Check OrderCandidate " + orderCandidate.Id, arbitrage);
                await _context.SaveChangesAsync();
                return;
            }



            //BuyUnitPrice * (OrgiAmount  - RemainAmount)  - Check Trade
            //var BuyNetPriceCm = buyResult.Item2.


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

        public async Task<Tuple<bool, Order>> BuyLimitOrderAsync(OrderCandidate orderCandidate, Arbitrage arbitrage)
        {

            if (!Config.ProcessTrades)
            {
                arbitrage.Comment = "BuyLimitOrderAsync skipped. ProcessTrades is not activated";
                _presenter.Warning(arbitrage.Comment);
                return new Tuple<bool, Order>(true, null);
            }

            _presenter.PrintOrderCandidate(orderCandidate);

            BuyResponse buyResponse = null;
            try
            {
                var pair = _coinmateLogic.GetLongPair(orderCandidate.Pair);
                if (!_coinmateLogic.Pairs.Any(p => p == pair))
                {
                    arbitrage.Comment = "Unsupported currency pair. Process cancel...";
                    _presenter.ShowError(arbitrage.Comment);

                    return new Tuple<bool, Order>(false, null);
                }

                buyResponse = await _coinmateLogic.BuyLimitOrderAsync(pair, orderCandidate.Amount, orderCandidate.UnitAskPrice, orderCandidate.Id);
            }
            catch (System.Exception ex)
            {
                arbitrage.Comment = $"Buylimit failed. {ex}. Process cancel...";

                _presenter.ShowPanic(arbitrage.Comment);
                return new Tuple<bool, Order>(false, null);
            }

            if (buyResponse == null)
            {
                arbitrage.Comment = $"Buyresponse is empty. Process cancel...";
                _presenter.ShowError(arbitrage.Comment);
                return new Tuple<bool, Order>(false, null);
            }

            if (buyResponse.error)
            {
                arbitrage.Comment = $"Buylimit failed. {buyResponse.errorMessage}. Process cancel...";
                _presenter.ShowError(arbitrage.Comment);
                return new Tuple<bool, Order>(false, null);
            }

            if (buyResponse.data == null || buyResponse.data <= 0)
            {
                arbitrage.Comment = $"Buylimit failed. Invalid order ID. Process cancel...";
                _presenter.ShowPanic(arbitrage.Comment);
                return new Tuple<bool, Order>(false, null);
            }

            _presenter.ShowInfo($"Waiting for buy confirmation");
            arbitrage.BuyOrderId = buyResponse.data;

            Order result = null;

            bool opComplete = System.Threading.SpinWait.SpinUntil(() =>
            {
                try
                {
                    Thread.Sleep(480);
                    result = _coinmateLogic.GetOrderByOrderIdAsync(buyResponse.data.Value).Result;
                    if (result != null)
                        _context.EnrichBuy(arbitrage, result);
                }
                catch (Exception ex)
                {
                    _presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
                }
                return result != null && (result.status == "FILLED" || result.status == "PARTIALLY_FILLED" || result.status == "CANCELLED");


            }, TimeSpan.FromMilliseconds(buyTimeoutInMs));

            var buySuccess = result != null && result.status != null && (result.status == "FILLED" || result.status == "PARTIALLY_FILLED");
            if (!buySuccess)
            {
                _presenter.ShowInfo($"Buylimit order was sent but could not be confirmed in time. Current state is {result?.status} Trying to cancel the order.");

                if (result?.status == "CANCELLED")
                {
                    if (result?.remainingAmount == null || result?.remainingAmount.Value == 0)
                    {
                        arbitrage.Comment = "Order was already cancelled successfully.Process cancel...";
                        _presenter.ShowInfo(arbitrage.Comment);

                        return new Tuple<bool, Order>(false, result);
                    }
                    else
                    {
                        orderCandidate.Amount -= result.remainingAmount.Value;
                        arbitrage.Comment = $"Order is cancelled but there is an open position, that we need to sell. Amount adjusted to {orderCandidate.Amount}.";
                        _presenter.ShowInfo(arbitrage.Comment);
                        return new Tuple<bool, Order>(true, result);
                    }
                }

                //OPENED state
                CancelOrderResponse buyCancelResult = null;
                try
                {
                    buyCancelResult = await _coinmateLogic.CancelOrderAsync(buyResponse.data.Value);
                }
                catch (System.Exception ex)
                {
                    _presenter.ShowPanic($"CancelOrderAsync failed. {ex}. Maybe buy was successful.");
                }

                if (buyCancelResult != null && buyCancelResult.data)
                {
                    arbitrage.Comment = "Order was cancelled successfully.Process cancel...";
                    _presenter.ShowInfo(arbitrage.Comment);

                    return new Tuple<bool, Order>(false, result);
                }
                else
                {
                    arbitrage.Comment = $"CancelOrderAsync  exited with wrong errorcode. Assuming the trade was finished.";
                    _presenter.ShowPanic(arbitrage.Comment);
                    return new Tuple<bool, Order>(true, result);
                }
            }

            if (result.status == "PARTIALLY_FILLED")
            {
                if (result.remainingAmount is null)
                {
                    arbitrage.Comment = $"Partial buy was done but remaing amount is not set. Don't know how much to sell. Please check the coinmate platform manually...Process cancel...";
                    _presenter.ShowPanic(arbitrage.Comment);
                    return new Tuple<bool, Order>(false, result);
                }

                orderCandidate.Amount -= result.remainingAmount.Value;

                _presenter.ShowInfo($"Successful partial {result.orderTradeType} {result.originalAmount}+{result.remainingAmount}={orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} ( UnitPrice: {result.price})  on {orderCandidate.BuyExchange} status {result.status}. SellAmount updated.");
            }
            else
            {
                _presenter.ShowInfo($"Successful {result.orderTradeType} {result.originalAmount}+{result.remainingAmount}={orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} ( UnitPrice: {result.price}) on {orderCandidate.BuyExchange} status {result.status}");
            }

            return new Tuple<bool, Order>(true, result);

        }



        public async Task<Tuple<bool, OrderResponse>> SellMarketAsync(OrderCandidate orderCandidate, Arbitrage arbitrage)
        {

            if (!Config.ProcessTrades)
            {
                arbitrage.Comment = "SellMarketAsync skipped. ProcessTrades is not activated";
                _presenter.Warning(arbitrage.Comment);
                return new Tuple<bool, OrderResponse>(true, null);
            }

            orderCandidate.Amount = Math.Round(orderCandidate.Amount, 6);

            if (orderCandidate.Amount <= 0)
            {
                arbitrage.Comment = "SellMarketAsync skipped. Amount too small";
                _presenter.Warning(arbitrage.Comment);
                return new Tuple<bool, OrderResponse>(true, null);
            }


            _presenter.ShowInfo("Let's sell");

            OrderResponse result = null;
            try
            {
                result = await _binanceLogic.SellMarketAsync(orderCandidate);

            }
            catch (System.Exception ex)
            {
                arbitrage.Comment = $"SellMarketAsync failed. {ex}. Please check binance manually.";
                _presenter.ShowPanic(arbitrage.Comment);
                return new Tuple<bool, OrderResponse>(false, null);

            }

            if (result == null)
            {
                arbitrage.Comment = $"SellMarketAsync failed. Result is null.";
                _presenter.ShowPanic(arbitrage.Comment);
                return new Tuple<bool, OrderResponse>(false, null);
            }

            _context.EnrichSell(arbitrage, result);

            _presenter.ShowInfo($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} {result.side} {result.type} OrderId {result.orderId} OCID {result.clientOrderId}  price: {result.price} symbol: {result.symbol} Qty: {result.executedQty}/{result.origQty} cumQty: {result.cummulativeQuoteQty}");

            if (result.status == "FILLED")
                _presenter.ShowInfo("Successfully sold");
            else
            {
                _presenter.ShowPanic("Check line above for problems");
            }

            return new Tuple<bool, OrderResponse>(result.status == "FILLED", result);
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