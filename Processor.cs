using System;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Trader;
using Trader.Binance;
using Trader.Coinmate;
using Trader.Sms;
using Trader.PostgresDb;

public class Processor
{
        private readonly PostgresContext _context;

    public Processor(PostgresContext context)
    {
        _context = context;
    }
    private int buyTimeoutInMs = 2500;

    public async Task ProcessOrderAsync(OrderCandidate orderCandidate)
    {

        if (!(orderCandidate.BuyExchange == nameof(Trader.Coinmate) && orderCandidate.SellExchange == nameof(Trader.Binance)))
        {
            Presenter.ShowError("Unsupported exchnages. Process cancel...");
            return;
        }

        var sms = new SmsLogic();


        var buyResult = await BuyLimitOrderAsync(orderCandidate);

        if (!buyResult.Item1)
        {
            await sms.SendSmsAsync($"BuyLimitOrderAsync failed. Status: {buyResult.Item2?.status} Check OrderCandidate " + orderCandidate.Id);
            return;

        }

        var sellResult = await SellMarketAsync(orderCandidate);

        if (!sellResult.Item1)
        {
            await sms.SendSmsAsync("SellMarketAsync failed. Check OrderCandidate " + orderCandidate.Id);
            return;
        }

        var successMessage = $"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} Arbitrage success. OCID {orderCandidate.Id} estNetProfit {orderCandidate.EstProfitNet} realNetProfit {sellResult.Item2.cummulativeQuoteQtyNum - orderCandidate.TotalAskPrice}";
        Presenter.ShowSuccess(successMessage);
        await sms.SendSmsAsync(successMessage);

    }

    public async Task<Tuple<bool, Order>> BuyLimitOrderAsync(OrderCandidate orderCandidate)
    {

        if (!Config.ProcessTrades)
        {
            Presenter.Warning("BuyLimitOrderAsync skipped. ProcessTrades is not activated");
            return new Tuple<bool, Order>(true, null);
        }

        Presenter.PrintOrderCandidate(orderCandidate);


        var coinmateLogic = new CoinmateLogic();



        BuyResponse buyResponse = null;
        try
        {
            var pair = coinmateLogic.GetLongPair(orderCandidate.Pair);
            if (!coinmateLogic.Pairs.Any(p => p == pair))
            {
                Presenter.ShowError("Unsupported currency pair. Process cancel...");
                return new Tuple<bool, Order>(false, null);
            }

            buyResponse = await coinmateLogic.BuyLimitOrderAsync(pair, orderCandidate.Amount, orderCandidate.UnitAskPrice, orderCandidate.Id);
        }
        catch (System.Exception ex)
        {
            Presenter.ShowPanic($"Buylimit failed. {ex}. Process cancel...");
            return new Tuple<bool, Order>(false, null);
        }

        if (buyResponse == null)
        {
            Presenter.ShowError($"Buyresponse is empty. Process cancel...");
            return new Tuple<bool, Order>(false, null);
        }

        if (buyResponse.error)
        {
            Presenter.ShowError($"Buylimit failed. {buyResponse.errorMessage}. Process cancel...");
            return new Tuple<bool, Order>(false, null);
        }

        if (buyResponse.data == null || buyResponse.data <= 0)
        {
            Presenter.ShowPanic($"Buylimit failed. Invalid order ID. Process cancel...");
            return new Tuple<bool, Order>(false, null);
        }

        Console.WriteLine($"Waiting for buy confirmation");

        Order result = null;

        bool opComplete = System.Threading.SpinWait.SpinUntil(() =>
        {
            try
            {
                Thread.Sleep(100);
                result = coinmateLogic.GetOrderByOrderIdAsync(buyResponse.data.Value).Result;
            }
            catch (Exception ex)
            {
                Presenter.ShowError($"GetOrderByOrderIdAsync failed. {ex} Retrying...");
            }
            return result != null && (result.status == "FILLED" || result.status == "PARTIALLY_FILLED" || result.status == "CANCELLED");


        }, TimeSpan.FromMilliseconds(buyTimeoutInMs));

        var buySuccess = result.status != null && (result.status == "FILLED" || result.status == "PARTIALLY_FILLED");
        if (!buySuccess)
        {
            Console.WriteLine($"Buylimit order was sent but could not be confirmed in time. Current state is {result?.status} Trying to cancel the order.");

            if (result?.status == "CANCELLED")
            {
                if (result?.remainingAmount == null || result?.remainingAmount.Value == 0)
                {
                    Console.WriteLine("Order was already cancelled successfully.Process cancel...");
                    return new Tuple<bool, Order>(false, result);
                }
                else
                {
                    Console.WriteLine("Order is cancelled but there is an open position, that we need to sell.");
                    orderCandidate.Amount -= result.remainingAmount.Value;
                    return new Tuple<bool, Order>(true, result);
                }
            }

            //OPENED state
            CancelOrderResponse buyCancelResult = null;
            try
            {
                buyCancelResult = await coinmateLogic.CancelOrderAsync(buyResponse.data.Value);
            }
            catch (System.Exception ex)
            {
                Presenter.ShowPanic($"CancelOrderAsync failed. {ex}. Maybe buy was successful.");
            }

            if (buyCancelResult != null && buyCancelResult.data)
            {
                Console.WriteLine("Order was cancelled successfully.Process cancel...");
                return new Tuple<bool, Order>(false, result);
            }
            else
            {
                Presenter.ShowPanic($"CancelOrderAsync  exited with wrong errorcode. Assuming the trade was finished.");
                return new Tuple<bool, Order>(true, result);
            }
        }

        if (result.status == "PARTIALLY_FILLED")
        {
            if (result.remainingAmount is null)
            {
                Presenter.ShowPanic($"Partial buy was done but remaing amount is not set. Don't know how much to sell. Please check the coinmate platform manually...Process cancel...");
                return new Tuple<bool, Order>(false, result);
            }

            orderCandidate.Amount -= result.remainingAmount.Value;
            Console.WriteLine($"Successful partial {result.orderTradeType} {result.originalAmount}+{result.remainingAmount}={orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} ( UnitPrice: {result.price})  on {orderCandidate.BuyExchange} status {result.status}. SellAmount updated.");
        }
        else
        {
            Console.WriteLine($"Successful {result.orderTradeType} {result.originalAmount}+{result.remainingAmount}={orderCandidate.Amount} {orderCandidate.Pair} for {orderCandidate.TotalAskPrice} ( UnitPrice: {result.price}) on {orderCandidate.BuyExchange} status {result.status}");
        }

        return new Tuple<bool, Order>(true, result);

    }
    public async Task<Tuple<bool, OrderResponse>> SellMarketAsync(OrderCandidate orderCandidate)
    {

        if (!Config.ProcessTrades)
        {
            Presenter.Warning("BuyLimitOrderAsync skipped. ProcessTrades is not activated");
            return new Tuple<bool, OrderResponse>(true, null);
        }
        Console.WriteLine("Let's sell");
        Presenter.PrintOrderCandidate(orderCandidate);


        var binanceLogic = new BinanceLogic();

        OrderResponse result = null;
        try
        {
            result = await binanceLogic.SellMarketAsync(orderCandidate);

        }
        catch (System.Exception ex)
        {
            Presenter.ShowPanic($"SellMarketAsync failed. {ex}. Please check binance manually.");
            return new Tuple<bool, OrderResponse>(false, null);

        }

        if (result == null)
            Presenter.ShowPanic($"SellMarketAsync failed. Result is null.");

        Console.WriteLine($"{DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss")} {result.side} {result.type} OrderId {result.orderId} OCID {result.clientOrderId}  price: {result.price} symbol: {result.symbol} Qty: {result.executedQty}/{result.origQty} cumQty: {result.cummulativeQuoteQty}");

        if (result.status == "FILLED")
            Console.WriteLine("Successfully sold");
        else
        {
            Presenter.ShowPanic("Check line above for problems");
        }

        return new Tuple<bool, OrderResponse>(result.status == "FILLED", result);
    }
}

